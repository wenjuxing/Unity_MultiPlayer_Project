using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Model;
using Serilog;
using Summer;
namespace GameServer.Mgr
{
   public class SpaceManager:Singleton<SpaceManager>
    {
        //地图字典
        private Dictionary<int, Space> Dict = new Dictionary<int, Space>();
        public SpaceManager() { }
        /// <summary>
        /// 初始化地图信息
        /// </summary>
        public void Init()
        {
            foreach (var kv in DataManager.Instance.Spaces)
            {
                Dict[kv.Key] = new Space(kv.Value);
            }
        }
        /// <summary>
        /// 通过地图Id获取地图
        /// </summary>
        /// <param name="spaceId"></param>
        /// <returns></returns>
        public Space GetSpace(int spaceId)
        {
            return Dict[spaceId];
        }
        /// <summary>
        /// 调用所有Space的Update
        /// </summary>
        public void Update()
        {
            foreach(var space in Dict.Values)
            {
                space.Update();
            }
        }
    }
}
