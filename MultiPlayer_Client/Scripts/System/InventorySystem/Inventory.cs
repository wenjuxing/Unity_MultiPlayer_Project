using GameClient;
using GameClient.Entities;
using Proto;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.InventorySystem
{
    /// <summary>
    /// 库存对象
    /// </summary>
    public class Inventory
    {
        public Character chr { get; private set; }
        //背包容量
        public int Capacity { get; private set; }
        //<插槽索引，物品对象>
        public ConcurrentDictionary<int, Item> itemDict { get; set; } = new ConcurrentDictionary<int, Item>();
        public Inventory(Character chr)
        {
            this.chr = chr;
        }
        /// <summary>
        /// 重新加载背包
        /// </summary>
        /// <param name="inventoryInfo"></param>
        public void ReLoad(InventoryInfo Info)
        {
            //清空字典
            itemDict.Clear();
            //设置容量
            this.Capacity = Info.Capacity;
            //把背包信息加入字典
            foreach (ItemInfo itemInfo in Info.List)
            {
                var item = new Item(itemInfo);
                itemDict.TryAdd(item.position, item);
                //Debug.Log(itemDict.TryAdd(item.position, item));
            }
        }
    }
}
