using GameClient.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
namespace GameClient
{
   public class Game
    {
        public static Actor GetUnit(int id)
        {
            return EntityManager.Instance.GetEntity<Actor>(id);
        }

        public static void StartCoroutine(IEnumerator routine)
        {
           UIManager.Instance.StartCoroutine(routine);
        }
        /// <summary>
        /// 获取范围内的单位角色列表
        /// </summary>
        /// <returns></returns>
        public static List<Actor> RangeUnit(Vector3 position, int range)
        {
            Predicate<Actor> mach = (e) =>
            {
                return Vector3.Distance(position, e.Position) <= range;
            };
            return EntityManager.Instance.GetEntities<Actor>(mach);
        }
    }
}
