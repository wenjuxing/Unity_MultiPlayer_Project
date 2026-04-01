using Serilog;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using GameServer.Mgr;
using GameServer.Core;
using GameServer.Fight;
using Summer;
using Google.Protobuf;

namespace GameServer.Model
{
    //场景
    public class Space
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SpaceDefine Def { get; set; }
        public FightMgr fightMgr;
        //当前场景中全部的角色
        private Dictionary<int, Character> CharacterDict = new Dictionary<int, Character>();
        //当前场景中全部Actor
        private Dictionary<int, Actor> ActorDict = new Dictionary<int, Actor>();

        public MonsterManager monsterManager = new MonsterManager();
        public SpawnManager spawnManager = new SpawnManager();

        public Space(SpaceDefine def) 
        {
            this.Def = def;
            this.Id = def.SID;
            this.Name = def.Name;
            this.fightMgr = new FightMgr(this);
            monsterManager.Init(this);
            spawnManager.Init(this);
        }
        /// <summary>
        /// Entity进入场景通用方法
        /// </summary>
        public void EntityEnter(Actor actor)
        {
            //记录到该场景的Actor字典
            ActorDict[actor.entityId] = actor;
            actor.OnEnterSpace(this);
            //把新进入的Actor广播给其他玩家
            var resp = new SpaceCharactersEnterResponse();
            resp.SpaceId = this.Id; //场景ID
            resp.ActorList.Add(actor.info);
            BroadCast(resp);
            //判断是否为主角
            if (actor is Character chr)
            {
                CharacterJoin(chr);
            }
        }
        /// <summary>
        /// 主角进入场景  
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="chr"></param>
        private void CharacterJoin(Character chr)
        {
            Log.Information("角色进入场景:" + chr.entityId);
            //加入场景中的角色字典
            CharacterDict[chr.entityId] = chr;
            //新上线的角色需要获取场景中全部的Actor
            SpaceEnterResponse ser = new SpaceEnterResponse();
            ser.Character = chr.info;
            //把所有Actor加入列表
            foreach (var kv in ActorDict)
            {
                ser.List.Add(kv.Value.info);
            }
            //统一发送一个列表
            chr.conn.Send(ser);

        }
        /// <summary>
        /// Entity离开场景通用方法
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        public void EntityLeave(Actor actor)
        {
            Log.Information("角色离开场景:" + actor.entityId);
            ActorDict.Remove(actor.entityId);
            //广播给其他玩家
            SpaceCharacterLeaveResponse resp = new SpaceCharacterLeaveResponse();
            resp.EntityId = actor.entityId;
            BroadCast(resp);
            //如果是主角
            if (actor is Character chr)
            {
                //把角色属性中的场景设为空      
                CharacterDict.Remove(chr.entityId);
            }
        }
        /// <summary>
        /// 广播更新entity信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void UpdateEntity(NEntitySync entitySync)
        {
            foreach (var kv in CharacterDict)
            {
                //如果是自己则覆盖数值
                if (kv.Value.entityId==entitySync.Entity.Id)
                {
                    kv.Value.EntityData = entitySync.Entity;
                }
                //如果是其他人则把数据发送给服务器
                else
                {
                    SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                    resp.EntitySync = entitySync;
                    kv.Value.conn.Send(resp);
                }
            }
        } 
        /// <summary>
        /// 调用刷怪管理器的Update
        /// </summary>
        public void Update()
        {
            this.spawnManager.Update();
            this.fightMgr.OnUpdate(Time.deltaTime);
        }
        /// <summary>
        /// 广播消息给所有角色
        /// </summary>
        public void BroadCast(IMessage msg)
        {
            foreach (var kv in CharacterDict)
            {
                kv.Value.conn.Send(msg);    
            }
        }
        /// <summary>
        /// 同场景位置传送
        /// </summary>
        public void TelePort(Actor actor,Vector3Int Pos,Vector3Int Dir=new Vector3Int())
        {
            //设置位置
            actor.Position = Pos;
            actor.Direction = Dir;
            //设置消息
            SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
            resp.EntitySync = new NEntitySync();
            resp.EntitySync.Entity = actor.EntityData;
            resp.EntitySync.Force = true;
            //广播消息
            BroadCast(resp);
        }
    }
}
