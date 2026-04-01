using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Model;
using Summer;

namespace GameServer.Mgr
{
    /// <summary>
    /// Entity管理器 (角色 怪物 NPC)
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        private int index = 1;
        //存储EntityId和对应的Entity
        private ConcurrentDictionary<int, Entity> AllEntities = new ConcurrentDictionary<int, Entity>();
        //存储SpaceId和对应场景下的Entity
        private ConcurrentDictionary<int, List<Entity>> SpaceEntities = new ConcurrentDictionary<int, List<Entity>>();

        /// <summary>
        /// 添加Entity
        /// </summary>
        public void AddEntity(int spaceId,Entity entity)
        {
            lock (this)
            {
                //由统一管理的对象分配EntityId
                entity.EntityData.Id = NewEntityId;
                //把entity加入entity字典
                AllEntities[entity.entityId] = entity;
                //把entity加入对应场景的列表
                if (!SpaceEntities.ContainsKey(spaceId))
                {
                    SpaceEntities[spaceId] = new List<Entity>();
                }
                //线程安全的添加Entity
                ForUnit(spaceId,(list)=> { list.Add(entity); });
            }
        }
        /// <summary>
        /// 移除Entity
        /// </summary>
        /// <param name="spaceId"></param>
        /// <param name="entity"></param>
        public void RemoveEntity(int spaceId,Entity entity)
        {
            lock (this)
            {
                AllEntities.TryRemove(entity.entityId,out Entity value);
                //线程安全的移除Entity
                ForUnit(spaceId, (list) => { list.Remove(entity); });
            }
        }
        /// <summary>
        /// 获取Entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
       public Entity GetEntity(int entityId)
        {
            if (AllEntities.TryGetValue(entityId, out Entity entity)) return entity;
            else return null;
        }
        /// <summary>
        /// 查询场景中的Entity对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spaceId"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public List<T> GetEntitylist<T>(int spaceId,Predicate<T> mach) where T : Entity
        {
            if (!SpaceEntities.TryGetValue(spaceId, out var list)) return null;
          return list?
                .OfType<T>()                           //筛选类型
                .Where(entity => mach.Invoke(entity))  //筛选条件符合的
                .ToList();                             //返回列表  
        }
        /// <summary>
        /// 目标Entity是否存在
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool Exit(int entityId)
        {
           return AllEntities.ContainsKey(entityId);
        }
        /// <summary>
        /// Entity列表的功能进行封装 确保线程安全
        /// </summary>
        public void ForUnit(int SpaceId,Action<List<Entity>> action)
        {
            if (SpaceEntities.TryGetValue(SpaceId,out var list))
            {
                if (list == null) return;
                //确保同一时间只有一个线程操作这个列表
                lock (list)
                {
                    action?.Invoke(list);
                }
            }
        }
        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldSpaceId"></param>
        /// <param name="newSpaceId"></param>
        public void ChangeSpace(Entity entity,int oldSpaceId,int newSpaceId)
        {
            //新旧场景Id相同则返回
            if (oldSpaceId == newSpaceId) return;
            //从旧的场景列表中移除
            ForUnit(oldSpaceId, (list) => { list.Remove(entity); });
            //加入新的场景列表
            ForUnit(newSpaceId, (list) => { list.Add(entity); });
        }
        public int NewEntityId
        {
            get {
                lock(this){
                    return index++; 
                }
            }
        }
        public void Update()
        {
            foreach(var entity in AllEntities.Values)
            {
                entity.Update();
            }
        }

    }
}
