using GameServer.Mgr;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
   public class Game
    {
        /// <summary>
        /// 获取Actor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Actor GetUnit(int id)
        {
            return EntityManager.Instance.GetEntity(id)as Actor;
        }
        /// <summary>
        /// 获取范围内的单位角色列表
        /// </summary>
        /// <returns></returns>
        public static List<Actor> RangeUnit(int spaceId,Vector3 position,int range)
        {
            Predicate<Actor> mach = (e) =>
            {
                return Vector3Int.Distance(position,e.Position)<=range;
            };
            return EntityManager.Instance.GetEntitylist(spaceId,mach);
        }
    }
}
