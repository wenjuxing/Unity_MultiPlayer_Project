using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Serilog;
using GameServer.Core;
using GameServer.Fight;

namespace GameServer.Service
{
   public class BattleService:Singleton<BattleService>
    {
        public void Start()
        {
            MessageRouter.Instance.Subscribe<SpellRequest>(_SpellRequest);
        }

        private void _SpellRequest(Connection conn, SpellRequest msg)
        {
            Log.Information($"技能释放请求{msg}");
            var chr = conn.Get<Session>().character;
            //施法者和消息发送者Id相同？
            if (chr.entityId!=msg.Info.CasterId)
            {
                Log.Error($"施法信息错误{msg}");
                return;
            }
            //把施法信息加入战斗管理器
            chr.Space.fightMgr.CastQueue.Enqueue(msg.Info);
        }
    }
}
