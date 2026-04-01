using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Summer.Network;
using Proto;
using Summer;
using GameServer.Model;
using Serilog;
using GameServer.Mgr;
using GameServer.Core;

namespace GameServer.Service
{    
    public class SpaceService: Singleton<SpaceService>
    {
        public void Start()
        {           
            //初始化地图
            SpaceManager.Instance.Init();
            MessageRouter.Instance.Subscribe<SpaceEntitySyncRequest>(_SpaceEntitySyncRequest);
        }
        /// <summary>
        /// 通过地图ID获取Space
        /// </summary>
        /// <param name="spaceId"></param>
        /// <returns></returns>
        public Space GetSpace(int spaceId)
        {
            return SpaceManager.Instance.GetSpace(spaceId);
        }
       
        private void _SpaceEntitySyncRequest(Connection conn, SpaceEntitySyncRequest msg)
        {
            //获取当前角色所在的地图
            var space = conn.Get<Session>().space;
            if (space == null) return;

            //请求同步的信息
            NetEntity netEntity = msg.EntitySync.Entity;
            //服务器中的实际位置
            Entity serEntity = EntityManager.Instance.GetEntity(netEntity.Id);
            //计算位置差
            float dict=Vector3Int.Distance(netEntity.Position,serEntity.Position);
            //使用服务器速度
            netEntity.Speed = serEntity.Speed;
            //限制最大时间差 防止长时间挂机不动后再次移动的移动距离很长
            float dt = Math.Min(serEntity.PositionTime,1);
            //计算限额
            float limit = serEntity.Speed * dt* 1.5f;
            if (float.IsNaN(dict) ||limit<dict)
            {
                SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                resp.EntitySync = new NEntitySync();
                resp.EntitySync.Entity = serEntity.EntityData;
                resp.EntitySync.Force = true;
                conn.Send(resp);
                return;
            }

            //更新需要同步消息
            space.UpdateEntity(msg.EntitySync);
        }
    }
}
