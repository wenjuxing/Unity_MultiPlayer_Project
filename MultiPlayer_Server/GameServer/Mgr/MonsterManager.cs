using GameServer.Model;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    /// <summary>
    /// 每个地图都使用Monster管理器
    /// </summary>
    public class MonsterManager 
    {
        //EntityId 怪物对象
        private Dictionary<int, Monster> _dict = new Dictionary<int, Monster>();

        private Space _space;

        public void Init(Space space)
        {
            this._space = space;
        }
        /// <summary>
        /// 创建怪物
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="level"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Monster Create(int tid,int level,Vector3Int pos,Vector3Int dir)
        {
            Monster monster = new Monster(tid,level,pos,dir);
            EntityManager.Instance.AddEntity(_space.Id,monster);
            monster.info.EntityId = monster.entityId;
            monster.info.SpaceId = _space.Id;
            _dict[monster.entityId] = monster;
            monster.Id = monster.entityId;
            this._space.EntityEnter(monster);
            return monster;
        }
    }
}
