using Assets.Scripts.U3d_scripts;
using GameClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InventoryDataMgr:SingletonBase<InventoryDataMgr>
{
    private List<Item> currentItemList = new List<Item>();
    /// <summary>
    /// 通过商品类型获取对应的商品列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<Item> GetItemListByType(ItemType type)
    {
        currentItemList.Clear();
        foreach (var item in GameApp.character.knapsack.itemDict.Values)
        {
            if (item.ItemType==type)
            {
                currentItemList.Add(item);
            }
            if (type==ItemType.All)
            {
                currentItemList.Add(item);
            }
        }
        return currentItemList;
    }
    /// <summary>
    /// 获取所有商品类型
    /// </summary>
    /// <returns></returns>
    public List<ItemType> GetAllItemType()
    {
        List<ItemType> types = new List<ItemType>();
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            types.Add(type);
        }
        return types;
    }
}
