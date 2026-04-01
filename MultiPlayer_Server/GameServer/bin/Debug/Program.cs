using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Service;
using System.Net;
using System.Net.Sockets;
using Proto;
using Google.Protobuf;
using Summer.Network;
using System.Threading;
using Common;
using Serilog;
using GameServer.Database;
using System.IO;
using GameServer.Mgr;
using Summer;
using GameServer.Model;
using GameServer.AI;
using GameServer.InventorySystem;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化日志环境
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Async(a=>a.Console())
            .WriteTo.File("logs\\GameServer-log.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Async(a=>a.File("logs\\GameServer-log.txt", rollingInterval: RollingInterval.Day))
            .CreateLogger();

            //读取JSON文件
            DataManager.Instance.Init();

            NetService netService = new NetService();
            netService.Start();
            Log.Information("网络服务完成");

            UserService userService = UserService.Instance;
            userService.Start();
            Log.Information("玩家服务启动完成");

            SpaceService spaceService = SpaceService.Instance;
            spaceService.Start();
            Log.Information("地图服务启动完成");

            BattleService.Instance.Start();
            Log.Information("战斗服务启动完成");

            ChatService.Instance.Start();
            Log.Information("聊天服务启动完成");

            Scheduler.Instance.Start();
            Log.Information("中心计时器启用");

            //Log.Information("{0}", DataManager.Instance.dialogueMains[1001].name);
            //Log.Information("{0}", DataManager.Instance.dialogueGroups[2001].triggerEvent);
            //Log.Information("{0}", DataManager.Instance.dialogueDatas[3001].content);
            //Log.Information("{0}", DataManager.Instance.dialogueOptions[4001].content);

           


            //MessageRouter.Instance.Start(8);
            
            //提前手动建表，调用一次即可
            //DataBase.SyncAllTables();

            //  Space space = SpaceManager.Instance.GetSpace(2);
            //  Monster monster= space.monsterManager.Create(1002,3,new Vector3Int(85000,0,60000),Vector3Int.zero);
            //  monster.AI = new MonsterAI(monster); 

            //每秒执行50次Update函数
            Scheduler.Instance.AddTask(() => 
            { EntityManager.Instance.Update();
                SpaceManager.Instance.Update();
            }, 0.02f);

            //获取新手村场景
            //ItemEntity.Create(1, 1001,10, Vector3Int.zero, Vector3Int.zero);
            //ItemEntity.Create(1, 1002,5, new Vector3Int(3000,0,3000), Vector3Int.zero);
            ItemEntity.Create(1, 1003,1, new Vector3Int(5000, 0, 4000), Vector3Int.zero);

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
