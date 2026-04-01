using GameServer.Model;
using Proto;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.InventorySystem
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
        private ConcurrentDictionary<int, Item> ItemMap { get; } = new ConcurrentDictionary<int, Item>();
        //默认为true确保首次可以执行
        private bool hasChanged = true;
        public Inventory(Character chr)
        {
            this.chr = chr;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="inventoryInfo"></param>
        public void Init(byte[] bytes)
        {
            //首次进入游戏？
            if (bytes == null)
            {
                this.Capacity = 24;
            }
            else
            {
                //把字节数组转换为proto对象(反序列化)
                InventoryInfo info = InventoryInfo.Parser.ParseFrom(bytes);
                Log.Information("反序列化:{0}",info);
                this.Capacity = info.Capacity;
                //创建物品
                foreach (var iteminfo in info.List)
                {
                    SetItem(iteminfo.Position,new Item(iteminfo));
                }
            }
        }
        /// <summary>
        /// 设置插槽物品 插槽索引从0开始
        /// </summary>
        /// <param name="position"></param>
        /// <param name="item"></param>
        public bool SetItem(int slotIndex, Item item)
        {
            //如果索引大于容量则停止设置
            if (slotIndex >= Capacity) return false;
            //标记数据发生变化
            hasChanged = true;
            //清空当前插槽
            if (item is null)
            {
                ItemMap.TryRemove(slotIndex, out var _value);
                if (_value != null) _value.position = -1;
                return true;
            }
            //设置插槽物品
            ItemMap[slotIndex] = item;
            item.position = slotIndex;
            return true;
        }
        /// <summary>
        /// 获取插槽物品
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TrySlotItem(int slotIndex, out Item item)
        {
            return ItemMap.TryGetValue(slotIndex, out item);
        }
        //背包信息
        private InventoryInfo _inventoryInfo;
        public InventoryInfo InventoryInfo
        {
            get 
            {
                if (_inventoryInfo == null)
                {
                    _inventoryInfo = new InventoryInfo();
                }
                _inventoryInfo.Capacity = Capacity;
                //如果数据有变化
                if (hasChanged)
                {
                    //重新拼装proto对象
                    _inventoryInfo.List.Clear();
                    foreach (var item in ItemMap.Values)
                    {
                        _inventoryInfo.List.Add(item.itemInfo);
                    }
                }
                return _inventoryInfo;
            }
        }
        /// <summary>
        /// 添加物品
        /// </summary>
        public bool AddItem(int itemId,int amount=1)
        {
            //物品信息存在？
            if (!DataManager.Instance.Items.TryGetValue(itemId, out var def))
            {
                Log.Information("物品信息不存在！");
                return false;
            }
            //检查剩余空间
            if (CalculateMaxRemainingQuantity(itemId) < amount) return false;
            //添加物品进入插槽
            while (amount>0)
            {
                //先查找存放相同物品的格子
                var sameItem = FindSameItemAndNotFull(itemId);
                if (sameItem!=null)
                {
                    //计算该格子还可以放多少个物品
                    var current = Math.Min(amount,sameItem.Capacity-sameItem.amount);
                    FindSameItemAndNotFull(itemId).amount += current;
                    //新增
                    amount -= current;
                }
                //查找空闲的格子
                else
                {
                    //空格子的索引
                    var index = FindEmptyIndex();
                    if (index>-1)
                    {
                        //计算本次可以存放的数量
                        var current = Math.Min(amount, def.Capicity);
                        SetItem(index,new Item(def,current,index));
                        amount -= current;
                    }
                    else
                    {
                        Log.Debug("没有空的物品槽");
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 检查存放该物品的最大数量
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private int CalculateMaxRemainingQuantity(int itemId)
        {
            //检查物品类型是否存在
            if (!DataManager.Instance.Items.TryGetValue(itemId, out var def))
            {
                Log.Information("物品id不存在:{0}", itemId);
                return 0;
            }
            //记录可用数量
            var quantity = 0;
            //遍历全部插槽
            for (var i = 0; i < Capacity; i++)
            {
                //如果插槽有物品
                if (ItemMap.TryGetValue(i, out var item))
                {
                    //如果物品类型相同
                    if (item.Id == itemId)
                    {
                        quantity += (item.Capacity - item.amount);
                    }
                }
                else
                {
                    quantity += def.Capicity;
                }
            }
            Log.Debug("Inventory：Entity[{0}] 物品[{1}]还能放入[{2}]个", chr.entityId, def.Name, quantity);
            return quantity;
        }
        /// <summary>
        /// 移除物品
        /// </summary>
        /// <returns></returns>
        public int RemoveItem(int itemId, int amount = 1)
        {
            int removedAmount = 0;

            while (amount > 0)
            {
                var item = FindSameItem(itemId);

                if (item == null)
                {
                    break;
                }
                // 判断要移除的数量是否大于物品的当前数量
                int currentAmount = Math.Min(amount, item.amount);
                item.amount -= currentAmount;
                removedAmount += currentAmount;
                amount -= currentAmount;
                //清空物品槽
                if (item.amount == 0)
                {
                    SetItem(item.position, null);
                }
            }

            return removedAmount;
        }
        /// <summary>
        /// 交换物品位置
        /// </summary>
        /// <returns></returns>
        public bool Exchange(int originSlotIndex, int targetSlotIndex)
        {
            //交换id不能相同
            if(originSlotIndex==targetSlotIndex) return false;
            //交换id不能小于零
            if(originSlotIndex<0 ||  targetSlotIndex<0) return false;
            //查找原始插槽物品
            if (!ItemMap.TryGetValue(originSlotIndex, out var item1)) return false;
            Log.Information("item1={0}",item1.Name);
            //查找目标插槽物品，如果为空，直接放置
            if (!ItemMap.TryGetValue(targetSlotIndex, out var item2))
            {
                SetItem(originSlotIndex, null);
                SetItem(targetSlotIndex, item1);
            }
            else
            {
                //如果物品类型相同
                if(item1.Id == item2.Id)
                {
                    //可移动的数量
                    int num = Math.Min(item2.Capacity - item2.amount, item1.amount);
                    // 如果原始物品数量小于等于可移动数量，将原始物品全部移动到目标插槽
                    if (item1.amount <= num)
                    {
                        item2.amount += item1.amount;
                        SetItem(originSlotIndex, null);
                    }
                    else
                    {
                        // 否则，不移动物品只修改数量
                        item2.amount += num;
                        item1.amount -= num;
                    }
                }
                //如果类型不同则交换位置
                else
                {
                    SetItem(targetSlotIndex, item1);
                    SetItem(originSlotIndex, item2);
                }
            }

            return true;
        }
        /// <summary>
        /// 丢弃物品
        /// </summary>
        public int Discard(int slotIndex, int amount = 1)
        {
            if (amount < 1) return 0;
            if (!ItemMap.TryGetValue(slotIndex, out var item)) return 0;
            //只丢弃一部分
            if (amount < item.amount)
            {
                item.amount -= amount;
                var newItem = new Item(item.Id, amount);
                ItemEntity.Create(chr.Space, newItem, chr.Position, Vector3Int.zero);
                return amount;
            }
            //全额丢弃
            SetItem(slotIndex, null);
            ItemEntity.Create(chr.Space, item, chr.Position, Vector3Int.zero);
            return item.amount;
        }
        /// <summary>
        /// 查询相同Id且未满的物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Item FindSameItemAndNotFull(int itemId)
        {
            return ItemMap.Values.FirstOrDefault(item =>
            item != null && item.Id == itemId && item.amount < item.Capacity);
        }
        /// <summary>
        /// 查找Id相同的物品
        /// </summary>
        /// <returns></returns>
        private Item FindSameItem(int itemId)
        {
            return ItemMap.Values.FirstOrDefault(item => item?.Id == itemId);
        }
        /// <summary>
        /// 查找空的格子
        /// </summary>
        /// <returns></returns>
        public int FindEmptyIndex()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (!ItemMap.ContainsKey(i)) return i;
            }
            return -1;
        }
    }
}
