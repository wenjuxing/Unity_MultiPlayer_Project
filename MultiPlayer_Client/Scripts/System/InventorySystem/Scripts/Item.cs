using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameClient
{
    /// <summary>
    /// 物品类型
    /// </summary>
    public enum ItemType
    {
        Consumable,      //消耗品
        Equipment,       //装备
        Material,         //材料
        All
    }
    /// <summary>
    /// 物品品质
    /// </summary>
    public enum Quality
    {
        Common,         //普通
        Uncommon,       //非凡
        Rare,           //稀有
        Epic,           //史诗
        Legendary,      //传说
        Artifact,       //神器 
    }
    /// <summary>
    /// 物品基类
    /// </summary>
    [Serializable]
    public class Item
    {
        public int Id { get; set; } //物品Id
        public string Name { get; set; } //物品名称
        public ItemType ItemType { get; set; } //物品类型
        public Quality Quality { get; set; } //物品品质
        public string Description { get; set; } //物品描述
        public int Capacity { get; set; } //物品容量
        public int BuyPrice { get; set; } //买入价格
        public int SellPrice { get; set; } //售出价格
        public string Sprite { get; set; } //物品图标路径
        public ItemDefine Def { get; private set; }
        private Sprite _sprite;
        public Sprite SpriteImage
        {
            get
            {
                if (_sprite==null)
                {
                    _sprite = Resources.Load<Sprite>(Sprite);
                }
                return _sprite;
            }
        }  //物品图标精灵
        public int amount;   //物品数量
        public int position; //所处位置

        //物品信息
        private ItemInfo _itemInfo;
        public ItemInfo itemInfo
        {
            get
            {
                if (_itemInfo == null)
                {
                    _itemInfo = new ItemInfo() { ItemId = Id };
                }
                _itemInfo.Amount = amount;
                _itemInfo.Position = position;
                return _itemInfo;
            }
        }
        public Item(int itemId, int amount = 1, int position = 0)
        : this(DataManager.Instance.Items[itemId], amount, position)
        {

        }
        public Item(ItemInfo itemInfo) : this(DataManager.Instance.Items[itemInfo.ItemId])
        {
            this.amount = itemInfo.Amount;
            this.position = itemInfo.Position;
        }

        public Item(ItemDefine _def, int amount = 1, int position = 0) : this(_def.ID, _def.Name, ItemType.Material, Quality.Common,
            _def.Description, _def.Capicity, _def.BuyPrice, _def.SellPrice, _def.Icon)
        {
            Def = _def;
            this.amount = amount;
            this.position = position;
            switch (Def.ItemType)
            {
                case "消耗品": this.ItemType = ItemType.Consumable; break;
                case "道具": this.ItemType = ItemType.Material; break;
                case "装备": this.ItemType = ItemType.Equipment; break;
            }
            switch (Def.Quality)
            {
                case "普通": this.Quality = Quality.Common; break;
                case "非凡": this.Quality = Quality.Uncommon; break;
                case "稀有": this.Quality = Quality.Rare; break;
                case "史诗": this.Quality = Quality.Epic; break;
                case "传说": this.Quality = Quality.Legendary; break;
                case "神器": this.Quality = Quality.Artifact; break;
            }
        }
        public Item(int id, string name, ItemType itemType, Quality quality, string description, int capicity, int buyPrice, int sellPrice, string sprite)
        {
            Id = id;
            Name = name;
            ItemType = itemType;
            Quality = quality;
            Description = description;
            Capacity = capicity;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            Sprite = sprite;
        }
        public virtual string GetTipText()
        {
            return $"<color=#ffffff>{Name}</color>\n" +
               $"<color=yellow>{Description}</color>\n\n" +
               $"<color=bulue>物品堆叠上限:{Capacity}</color>";
        }
    }
}
