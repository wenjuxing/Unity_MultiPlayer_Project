# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Development

This is a **.NET Framework 4.7.2** MMORPG game server written in C#. Open `MMO-SERVER.sln` in Visual Studio 2019+ or Rider to work on the solution.

- **Restore NuGet packages**: Run `nuget restore` or let Visual Studio auto-restore on build.
- **Build**: Build the solution in Visual Studio (Debug/Release, AnyCPU).
- **Run the server**: Set `GameServer` as the startup project and press F5, or run `GameServer/bin/Debug/GameServer.exe` directly.
- **No automated tests** — testing is done by connecting a Unity client and playing through the game.
- **NetClient** is a minimal console test client for sending raw protobuf messages; not used in production.

## Solution Structure

Three projects in `MMO-SERVER.sln`:

| Project | Type | Purpose |
|---------|------|---------|
| `Common` | Library | Shared networking, scheduler, protobuf messages, serialization |
| `GameServer` | Console EXE | Main game server — all game logic |
| `NetClient` | Console EXE | Minimal test client for debugging network messages |

## Architecture Overview

This is the **server-authoritative** backend for a client-server MMORPG. All game state lives on the server. The Unity client (separate repo) sends requests; the server validates, mutates state, and broadcasts results to affected clients. Communication is over raw TCP sockets using Google Protobuf serialization with a length-prefixed framing protocol.

### Network Layer (`Common/Network/`)

- **`TcpServer`** (`Common/Network/Server/TcpServer.cs`) — Low-level TCP listener. Accepts connections on `0.0.0.0:32510`, wraps each socket in a `Connection` object, fires `Connected`/`Disconnected` events.
- **`Connection`** (`Common/Network/Connection.cs`) — Wraps a TCP socket. Length-prefixed framing (4-byte int header + protobuf body). Send/receive with async socket operations. Has a generic `Get<T>()`/`Set<T>()` bag for attaching per-connection state (e.g., `Session`).
- **`MessageRouter`** (`Common/Network/MessageRouter.cs`) — **Multi-threaded message dispatcher** (singleton). Uses a producer-consumer pattern: messages arrive on IO threads, are enqueued, then dequeued by a configurable pool of worker threads. Subscribe/Off/Fire pattern keyed by protobuf type name (string). **Recursive message processing**: when firing, also reflects into nested protobuf sub-messages and fires those — so a handler for a parent message type also sees child message types.
- **`SocketReceiver`** (`Common/Network/SocketReceiver.cs`) — Async socket receive loop that reassembles the length-prefixed stream into complete protobuf messages, then calls `MessageRouter.AddMessage()`.
- **`ProtoHelper`** / **`DataSerializer`** — Serialization utilities on top of protobuf.
- Protobuf message definitions live in `Common/proto/Message.cs` (auto-generated from `message.proto`) under the `Proto` namespace.

### Service Layer (`GameServer/Service/`)

Each service is a `Summer.Singleton<T>` that subscribes to specific protobuf message types in its `Start()` method:

- **`NetService`** — Starts the `TcpServer` on port 32510, starts `MessageRouter` with 10 worker threads. Manages heartbeat: 5s timer checks all connections, disconnects those silent >10s. On connect: creates a new `Session` and attaches it to the `Connection`. On disconnect: removes character from space and `CharacterManager`.
- **`UserService`** — **Largest service**. Handles: login, register, character create/delete/list, game enter, inventory (placement/use/discard/request), item pickup, task (accept/update/submit/abandon/data), dialogue (request/update), shop (buy), currency (get), revive.
- **`SpaceService`** — Initializes `Space` instances from `SpaceDefine` JSON. Handles `SpaceEntitySyncRequest` with **anti-cheat validation**: compares client-claimed position vs server position, calculates max allowed movement based on speed × time × 1.5 multiplier, and forces a correction (`Force = true`) if the client exceeds the limit.
- **`BattleService`** — Receives `SpellRequest`, validates caster matches sender, enqueues `CastInfo` to the space's `FightMgr.CastQueue`.
- **`ChatService`** — Handles `ChatRequest`, broadcasts `ChatResponse` to all characters in the space.

### Entity System (`GameServer/Model/`)

Hierarchy: `Entity` → `Actor` → `Character` / `Monster` / `ItemEntity`

- **`Entity`** — Base class wrapping a protobuf `NetEntity`. Tracks position as `Vector3Int` (integer, ×1000 fixed-point), direction, speed, state. Has an auto-incrementing `entityId` assigned by `EntityManager`.
- **`Actor`** — Anything that fights. Has HP/MP/properties wrapped in a `NetActor` protobuf, an `AttributesAssembly` for computed stats, `SkillManager` for skill state machines, and `Spell` (the spell executor). Handles `RecvDamage()`, `Die()`, `SetHP()`/`SetMP()` (broadcasts `PropertyUpdate`), `Revive()`, and cross-scene `TelePortSpace()`.
- **`Character`** — Player character. Adds `Knapsack` (inventory), `TaskManager`, `DialogueManager`, `StorageManager`, `ShopManager`. Tied to a `Connection` via `Session`. Has `playerId` linking to `DbPlayer` in MySQL.
- **`Monster`** — Enemy NPC. Created by `MonsterManager` from `SpawnDefine` data via `SpawnManager`/`Spawner`. Has an `AIBase` (default: `MonsterAI`).
- **`ItemEntity`** — Dropped items in the world. Created via `ItemEntity.Create()` factory.
- **`Space`** — **Scene container** (equivalent to a map/zone). Owns `CharacterDict`, `ActorDict`, `FightMgr`, `MonsterManager`, `SpawnManager`. Key methods:
  - `EntityEnter(Actor)` — registers actor, broadcasts `SpaceCharactersEnterResponse` to all. For `Character`: sends full actor list via `SpaceEnterResponse`.
  - `EntityLeave(Actor)` — unregisters, broadcasts `SpaceCharacterLeaveResponse`.
  - `UpdateEntity(NEntitySync)` — broadcasts entity sync to all characters except the sender (who gets their own data merged server-side).
  - `BroadCast(IMessage)` — sends a message to every `Character` in the space.
  - `Update()` — called at 50Hz, ticks `SpawnManager` and `FightMgr`.
  - `TelePort(Actor, pos, dir)` — sets position and broadcasts a force-sync.

### Manager Classes (`GameServer/Mgr/`)

- **`EntityManager`** — Thread-safe `ConcurrentDictionary<int, Entity>` globally + per-space lists. Auto-assigns incrementing `entityId`. Handles cross-space movement via `ChangeSpace()`. `Update()` iterates all entities each tick.
- **`CharacterManager`** — `ConcurrentDictionary<int, Character>` of all online characters. `Create()` loads from `DbCharacter` DB row, registers in `EntityManager`. `Remove()` on disconnect. **Auto-saves every 5 seconds**: writes position, knapsack inventory to MySQL via FreeSql.
- **`SpaceManager`** — Initializes `Space` instances from `DataManager.Spaces` on startup. `GetSpace(spaceId)` for lookup. `Update()` delegates to each Space.
- **`DataManager`** — Loads all static game data from JSON files (units, skills, items, spaces, spawns, shops, tasks, dialogue). Same JSON schema as the client-side `DataManager`.
- **`MonsterManager`** / **`SpawnManager`** / **`Spawner`** — Monster lifecycle: `SpawnManager` owns `Spawner` instances per spawn point, each with cooldown/respawn logic. `MonsterManager.Create()` instantiates monsters from `UnitDefine`.

### Battle System (`GameServer/Fight/` + `GameServer/Battle/`)

- **`FightMgr`** — One per `Space`. Each tick (`OnUpdate`):
  1. Dequeues `CastQueue` → runs `RunCast()` for each spell
  2. Updates `Missile` projectiles
  3. `BroadCastSpell()` — sends accumulated `SpellResponse` to the space
  4. `BroadCastDamage()` — sends accumulated `DamageResponse`
  5. `BroadCastProperties()` — sends accumulated `PropertyUpdateResponse`
- **`Skill`** (`GameServer/Battle/Skill.cs`) — State machine: `None` → `Casting` (intonation) → `Active` (hit frames via `HitDelay[]`) → `Coolding` (CD) → `None`. Supports unit-target, point-target, no-target. Can fire `Missile` projectiles. **Damage formula**: `(AD + ATK×ADC) × (1 - DEF/(DEF + 400 + 85×Level))` + same for AP. Crit: random roll vs CRI%, forced crit after N non-crits. Hit: random roll vs (HitRate − DodgeRate).
- **`Spell`** (`GameServer/Fight/Spell.cs`) — Spell executor called by `FightMgr.RunCast()`. Handles the cast flow: validate → consume MP → start skill state machine → apply damage on hit frames.
- **`Missile`** — Projectile that moves toward target each frame, triggers `Skill.OnHit()` on arrival.
- **`AttributesAssembly`** / **`Attributes`** — Computed attribute system. `AttributesAssembly.Init(Actor)` computes final stats from base attributes + equipment + buffs.

### Database Layer (`GameServer/Database/`)

- **FreeSql ORM** with MySQL 8.0. Connection: `127.0.0.1:3306`, database `mmogame`, user `root`.
- **`DbPlayer`** — Player account (username, password, coin balance).
- **`DbCharacter`** — Character data (name, job, level, exp, position, knapsack as protobuf bytes).
- **`DataBase.SyncAllTables()`** — Manual schema migration, call once on first run.
- Characters auto-save every 5 seconds via `CharacterManager.Save()` (positions + inventory).

### Core Systems (`Common/` + `GameServer/Core/`)

- **`Scheduler`** (`Common/Scheduler.cs`) — **Central game timer**. Runs at 50 FPS (20ms tick). Supports delayed tasks (`AddTask(action, delaySeconds)`), repeating tasks (`AddTask(action, delay, interval, repeatCount)`), and per-frame callbacks (`Update(action)`). Thread-safe add/remove via concurrent queues. Powers the server game loop.
- **`Time`** — Game time tracking: `Time.time` (seconds since server start), `Time.deltaTime` (seconds since last tick). Updated by `Scheduler`.
- **`Singleton<T>`** (`Common/Singleton.cs`) — Thread-safe lazy singleton. Base class for most manager classes.
- **`Vector3Int`** / **`Vector3`** — Integer and float vector math. `Vector3Int` matches the client's protobuf `Vec3` (×1000 fixed-point). `Vector3Int.Distance()` used for range checks.
- **`Session`** — Per-connection bag: holds `Character`, `Space` (via character), `DbPlayer`.
- **`Game`** — Static helpers: `GetUnit(id)` looks up an `Actor` by entityId, `RangeUnit(spaceId, pos, range)` returns actors within a radius.
- **`MathC`** — Float comparison with epsilon tolerance.

### FSM System (`GameServer/Core/FSM/`)

- **`FsmSystem`** / **`State`** — Generic finite state machine used by the AI system (`MonsterAI`).

### Key Data Flow

1. **Startup** (`Program.Main()`): Init Serilog logging → `DataManager.Init()` (load JSON) → `NetService.Start()` (TCP listen + MessageRouter) → `UserService.Start()` (subscribe handlers) → `SpaceService.Start()` (init spaces) → `BattleService.Start()` → `ChatService.Start()` → `Scheduler.Start()` → register 50Hz update loop → idle spin.
2. **Client Connect**: TCP accept → `Connection` created → `Session` attached → heartbeat tracking starts.
3. **Login**: `UserLoginRequest` → validate username/password against `DbPlayer` → store `DbPlayer` in Session → respond `UserLoginResponse`.
4. **Enter Game**: `GameEnterRequest` → load `DbCharacter` from DB → `CharacterManager.Create()` (instantiate Character, register in EntityManager) → `Space.EntityEnter()` (broadcast to existing players, send full actor list to newcomer).
5. **Entity Sync**: Client sends `SpaceEntitySyncRequest` at 10Hz → `SpaceService` validates against server position (anti-cheat) → `Space.UpdateEntity()` broadcasts to other players.
6. **Combat**: Client sends `SpellRequest` → `BattleService` enqueues to `FightMgr.CastQueue` → next tick: `RunCast()` → `Spell.RunCast()` → `Skill.Use()` → state machine ticks → `Skill.OnHit()` → `TakeDamaged()` (damage formula + crit/dodge) → target `RecvDamage()` → `SetHP()` enqueues `PropertyUpdate` → damage enqueued → all broadcast at end of tick.
7. **Character Save**: `Scheduler` fires every 5s → `CharacterManager.Save()` → writes position + knapsack to `DbCharacter` rows via FreeSql `UpdateAsync`.

### Server Port & Protocol

- **Port**: `32510` (TCP)
- **Framing**: 4-byte little-endian length prefix + protobuf body
- **Heartbeat**: Client sends `HeartBeatRequest`, server replies `HeartBeatResponse`. Connections silent >10s are dropped.

### Third-Party Dependencies

| Package | Purpose |
|---------|---------|
| Google.Protobuf 3.21 | Network message serialization |
| FreeSql 3.5 + Provider.MySql | ORM for MySQL |
| MySql.Data 8.0 | MySQL ADO.NET driver |
| Newtonsoft.Json 13.0 | JSON data loading (shared with client) |
| Serilog 4.3 + Sinks (Console, File, Async) | Structured logging |
| Portable.BouncyCastle 1.9 | Crypto (MySQL auth) |
| K4os.Compression.LZ4 | Compression (MySQL protocol) |
