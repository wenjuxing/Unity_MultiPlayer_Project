using GameClient.Entities;
using Proto;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.Entities
{
    public class EntityManager : Singleton<EntityManager>
    {
        public EntityManager() { }
        //entityId对应的Entity
        //线程安全的字典
        private ConcurrentDictionary<int, Entity> _dict = new ConcurrentDictionary<int, Entity>();

        /// <summary>
        /// 添加entity
        /// </summary>
        /// <param name="entity"></param>
        private void AddEntity(Entity entity)
        {
            Debug.Log("AddEntity:" + entity.entityId);
            _dict[entity.entityId] = entity;
        }
        /// <summary>
        /// 移除Entity
        /// </summary>
        /// <param name="entityId"></param>
        public void RemoveEntity(int entityId)
        {
            Debug.Log("RemoveEntity:" + entityId);
            _dict.TryRemove(entityId,out Entity entity);
            //触发角色离开场景事件
            Kaiyun.Event.FireOut("CharacterLeave", entityId); 
        }
        /// <summary>
        /// 创建游戏实体时加入Entity管理器
        /// </summary>
        /// <param name="info"></param>
        public void OnEntityEnter(NetActor info)
        {
            if (info.EntityType == EntityType.Character)
            {
                AddEntity(new Character(info));
            }
            if (info.EntityType == EntityType.Monster)
            {
                AddEntity(new Monster(info));
            }
            if (info.EntityType==EntityType.Item)
            {
                AddEntity(new ItemEntity(info));
            }
            //调用主线程执行的创建游戏角色事件
            Kaiyun.Event.FireOut("CharacterEnter", info);
        }
        /// <summary>
        /// 处理同步信息
        /// </summary>
        /// <param name="entitySync"></param>
        public void OnEntitySync(NEntitySync entitySync)
        {
            Debug.Log("OnEntitySync:" + entitySync.Entity);
            if (_dict.TryGetValue(entitySync.Entity.Id, out Entity entity))
            {
                //把entity的信息同步到客户端的Entity中
                entity.EntityData = entitySync.Entity;
                entity.State = entitySync.State;
                //触发同步事件
                Kaiyun.Event.FireOut("EntitySync", entitySync);
            }
        }
        /// <summary>
        /// 通过Id获取Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public T GetEntity<T>(int entityId)where T:Entity
        {
            if (_dict.TryGetValue(entityId,out Entity entity))
            {
                return (T)entity;
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// 清空Entity字典
        /// </summary>
        public void Clear()
        {
            //销毁场景中所有的角色和NPC
            foreach (var entity in _dict.Values)
            {
                if (entity is Actor actor)
                {
                    GameObjectManager.Instance.CharacterLeave(actor.entityId);
                }
            }
            //清空字典
            _dict.Clear();
        }
        /// <summary>
        /// 获取Entity列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mach"></param>
        /// <returns></returns>
        public List<T> GetEntities<T>(Predicate<T> mach)
        {
           return _dict.Values.OfType<T>()
                .Where(e => mach.Invoke(e))
                .ToList();
        }
        /// <summary>
        /// 次方法由主线程调用
        /// </summary>
        /// <param name="delta"></param>
        public void OnUpdate(float delta)
        {
            foreach (var entity in _dict.Values)
            {
                var actor = entity as Actor;
                actor.SkillMgr?.OnUpdate(delta);
            }
        } 
    }
}
