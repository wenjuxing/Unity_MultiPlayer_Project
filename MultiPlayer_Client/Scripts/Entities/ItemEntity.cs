using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Entities
{
    public class ItemEntity : Actor
    {
        //真正的物品对象
        public Item Item { get; set; }
        public ItemEntity(NetActor info):base(info)
        {
            this.Item = new Item(info.ItemInfo);
        }
    }
}
