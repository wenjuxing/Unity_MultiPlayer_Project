using GameServer.Core;
using GameServer.Mgr;
using Proto;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    /// <summary>
    /// 聊天服务
    /// </summary>
    public class ChatService : Singleton<ChatService>
    {
        public void Start()
        {
            MessageRouter.Instance.Subscribe<ChatRequest>(_ChatRequest);
        }
        /// <summary>
        /// 聊天请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _ChatRequest(Connection conn, ChatRequest msg)
        {
            //获取发送者
            var chr = conn.Get<Session>().character;
            //转发消息到客户端
            ChatResponse resp = new ChatResponse();
            resp.SenderId = chr.entityId;
            resp.TextValue = msg.TextValue;
            //广播给场景中所有的客户端
            chr.Space.BroadCast(resp);

            //场景切换
            if (msg.TextValue=="新手村")
            {
                var space = SpaceManager.Instance.GetSpace(1);
                chr.TelePortSpace(space,Vector3Int.zero);
            }
            if (msg.TextValue == "森林")
            {
                var space = SpaceManager.Instance.GetSpace(2);
                chr.TelePortSpace(space,new Vector3Int(85000,5000,65000));
            }
            if (msg.TextValue == "位置1")
            {
                var space = SpaceManager.Instance.GetSpace(2);
                chr.TelePortSpace(space, new Vector3Int(80000, 5000, 60000));
            }
        }
    }
}
