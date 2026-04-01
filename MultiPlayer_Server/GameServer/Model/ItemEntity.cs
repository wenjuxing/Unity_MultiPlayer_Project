using GameServer.InventorySystem;
using GameServer.Mgr;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    /// <summary>
    /// 场景中的物品
    /// </summary>
    public class ItemEntity : Actor
    {
        //真正的物品对象
        public Item Item { get; set; }
        public ItemEntity(EntityType type, int tid, int level, Vector3Int position, Vector3Int direction) : base(type, tid, level, position, direction)
        {

        }
        /// <summary>
        /// 在场景创建物品
        /// </summary>
        /// <param name="space"></param>
        /// <param name="item"></param>
        /// <param name="Pos"></param>
        /// <param name="Dir"></param>
        public static ItemEntity Create(Space space,Item item,Vector3Int Pos,Vector3Int Dir)
        {
            //设置网络传输数据
            var entity = new ItemEntity(EntityType.Item,0,0,Pos,Dir);
            entity.Item = item;
            entity.info.ItemInfo = entity.Item.itemInfo;
            //加入entity管理器
            EntityManager.Instance.AddEntity(space.Id, entity);
            //物品进入场景
            space.EntityEnter(entity);
            return entity;
        }
        public static ItemEntity Create(int spaceId,int itemId,int amount, Vector3Int Pos, Vector3Int Dir)
        {
            Space space = SpaceManager.Instance.GetSpace(spaceId);
            var item = new Item(itemId, amount);
            return Create(space,item,Pos,Dir); 
        }
    }
}
