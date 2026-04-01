using System;
using System.Collections.Generic;
using Proto;
using Summer.Network;
using Serilog;
using GameServer.Model;
using System.Threading;
using GameServer.Mgr;
using GameServer.Core;

namespace GameServer.Service
{
    /// <summary>
    /// 网络服务模块
    /// </summary>
    public class NetService
    {
        private TcpServer tcpServer;
        //心跳响应
        private HeartBeatResponse beatResponse = new HeartBeatResponse();
        //记录最后一次的心跳时间
        private Dictionary<Connection, DateTime> heartBeatPairs = new Dictionary<Connection, DateTime>();
        public NetService()
        {
            //"0.0.0.0表示监听所有网络接口"
            tcpServer = new TcpServer("0.0.0.0",32510);
            tcpServer.Connected += OnClientConnected;
            tcpServer.Disconnected += OnDisconnected;
        }
   
        /// <summary>
        /// 开启网络监听
        /// </summary>
        public void Start()
        {
            tcpServer.Start();
            MessageRouter.Instance.Start(10);
            MessageRouter.Instance.Subscribe<HeartBeatRequest>(_HeartBeatRequest);
            //创建一个计时器
            Timer timer = new Timer(TimerCallback,null,TimeSpan.Zero,TimeSpan.FromSeconds(5));
        }
        void TimerCallback(object state)
        {
            Log.Information("执行检查");
            foreach (var kv in heartBeatPairs)
            {
                if ((DateTime.Now - kv.Value).TotalSeconds > 10)
                {
                    //关闭掉线的网络连接
                    kv.Key.Close();
                    heartBeatPairs.Remove(kv.Key);
                }
            }
        }

        /// <summary>
        /// 收到心跳包
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        private void _HeartBeatRequest(Connection conn, HeartBeatRequest msg)
        {
            //记录
            heartBeatPairs[conn] = DateTime.Now;
            //Log.Information($"收到心跳包{conn}");
            //向客户端回复心跳响应
            conn.Send(beatResponse);
        }

        /// <summary>
        /// 客户端连接成功后的回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="socket"></param>
        private void OnClientConnected(Connection conn)
        {
            heartBeatPairs[conn] = DateTime.Now;
            conn.Set<Session>(new Session());
            Log.Information("有客户端接入"); 
        } 
        /// <summary>
        /// 客户端断开连接的回调函数
        /// </summary>
        /// <param name="sender"></param>
        private void OnDisconnected(Connection conn)
        {
            heartBeatPairs.Remove(conn);
            Log.Information($"断开连接{conn}");
            var chr = conn.Get<Session>().character;
            var space = chr?.Space;
            if (space!=null)
            {
                space.EntityLeave(chr);

                //从角色管理器中移除断线的角色
                CharacterManager.Instance.Remove(chr.Id);
            }
        }
    }
}
