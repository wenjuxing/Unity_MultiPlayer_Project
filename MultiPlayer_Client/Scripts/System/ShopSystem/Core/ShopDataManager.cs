using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopDataManager : SingletonBase<ShopDataManager>
{
    public Dictionary<int, BaseGoods> _goodsDict = new Dictionary<int, BaseGoods>();
    public Dictionary<int, PropGoods> _propGoodsDict = new Dictionary<int, PropGoods>();
    public Dictionary<int, EquipGoods> _equipGoodsDict = new Dictionary<int, EquipGoods>();
    public Dictionary<int, SkinGoods> _skinGoodsDict = new Dictionary<int, SkinGoods>();
    public Dictionary<int, ConsumeGoods> _consumeGoodsDict = new Dictionary<int, ConsumeGoods>();
    public List<BaseGoods> _goodsList = new List<BaseGoods>();
    private void Awake()
    {
        DataManager.Instance.Init();
        LoadShopConfigs();
        //Debug.Log(_goodsList.Count);
    }
    public void LoadShopConfigs()
    {
        // 先清空字典，避免重复加载
        _goodsDict.Clear();
        _propGoodsDict.Clear();
        _equipGoodsDict.Clear();
        _skinGoodsDict.Clear();
        _goodsList.Clear();
        foreach (var goods in DataManager.Instance.ShopItems)
        {
            //加载数据到字典
            switch (goods.Value.ShopType)
            {
                case "道具":
                    PropGoods propGoods = new PropGoods();
                    propGoods.Id = goods.Value.id;
                    propGoods.Name = goods.Value.name;
                    propGoods.Price = goods.Value.Price;
                    propGoods.IsPurchased = goods.Value.IsPurchased;
                    propGoods.IsLimitPurchase = goods.Value.IsLimitPurchase;
                    propGoods.IconPath = goods.Value.IconPath;
                    propGoods.Type = GoodsType.Prop;
                    _goodsDict.Add(goods.Key, propGoods);
                    _propGoodsDict.Add(goods.Key, propGoods);
                    for(int i = 0; i < goods.Value.Count;i++)
                    {
                        //加载数据到列表中
                        _goodsList.Add(propGoods);
                    }
                    break;
                case "装备":
                    EquipGoods equipGoods = new EquipGoods();
                    equipGoods.Id = goods.Value.id;
                    equipGoods.Name = goods.Value.name;
                    equipGoods.Price = goods.Value.Price;
                    equipGoods.IsPurchased = goods.Value.IsPurchased;
                    equipGoods.IsLimitPurchase = goods.Value.IsLimitPurchase;
                    equipGoods.IconPath = goods.Value.IconPath;
                    equipGoods.Type = GoodsType.Equip;
                    equipGoods.Attack = goods.Value.Attack;
                    equipGoods.Defense = goods.Value.Defense;
                    _goodsDict.Add(goods.Key, equipGoods);
                    _equipGoodsDict.Add(goods.Key, equipGoods);
                    for (int i = 0; i < goods.Value.Count; i++)
                    {
                        //加载数据到列表中
                        _goodsList.Add(equipGoods);
                    }
                    break;
                case "皮肤":
                    SkinGoods skinGoods = new SkinGoods();
                    skinGoods.Id = goods.Value.id;
                    skinGoods.Name = goods.Value.name;
                    skinGoods.Price = goods.Value.Price;
                    skinGoods.IsPurchased = goods.Value.IsPurchased;
                    skinGoods.IsLimitPurchase = goods.Value.IsLimitPurchase;
                    skinGoods.IconPath = goods.Value.IconPath;
                    skinGoods.Type = GoodsType.Skin;
                    skinGoods.RoleName = goods.Value.RoleName;
                    _goodsDict.Add(goods.Key, skinGoods);
                    _skinGoodsDict.Add(goods.Key, skinGoods);
                    for (int i = 0; i < goods.Value.Count; i++)
                    {
                        //加载数据到列表中
                        _goodsList.Add(skinGoods);
                    }
                    break;
                  case "消耗品":
                    ConsumeGoods consumeGoods = new ConsumeGoods();
                    consumeGoods.Id = goods.Value.id;
                    consumeGoods.Name = goods.Value.name;
                    consumeGoods.Price = goods.Value.Price;
                    consumeGoods.IsPurchased = goods.Value.IsPurchased;
                    consumeGoods.IsLimitPurchase = goods.Value.IsLimitPurchase;
                    consumeGoods.IconPath = goods.Value.IconPath;
                    consumeGoods.Type = GoodsType.Consumable;
                    _goodsDict.Add(goods.Key, consumeGoods);
                    _consumeGoodsDict.Add(goods.Key, consumeGoods);
                    for (int i = 0; i < goods.Value.Count; i++)
                    {
                        //加载数据到列表中
                        _goodsList.Add(consumeGoods);
                    }
                    break;
            }

        }
    }
    /// <summary>
    /// 获取所有的商品类型
    /// </summary>
    /// <returns></returns>
    public List<GoodsType> GetAllGoodsTypes()
    {
        if (_goodsDict.Count == 0)
        {
            Debug.LogError("商品字典为空，返回空类型列表！");
            return new List<GoodsType>();
        }
        //通过类型进行区分，通过Distinct去重，OrderBy排序，最后转换为列表
        List<GoodsType> typeList= _goodsDict.Values.Select(t => t.Type).
            Distinct().OrderBy(type => type).ToList();
        typeList.Add(GoodsType.All);
        return typeList;
    }
    /// <summary>
    /// 通过类型去获取商品
    /// </summary>
    /// <returns></returns>
    public List<BaseGoods> GetGoodsByType(GoodsType type)
    {
        if (type == GoodsType.All) return _goodsList;
        return _goodsList.Where(t=>t.Type==type).ToList();
    }
}
