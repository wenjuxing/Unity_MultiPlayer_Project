using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinGoods : BaseGoods
{
    public string RoleName;
    public override bool Buy()
    {
        if (IsPurchased && IsLimitPurchase)
        {
            Debug.Log("本商品限购一件");
            return false;
        }
        IsPurchased = true;
        return true;
    }

    public override string GetExtInfo()
    {
        throw new System.NotImplementedException();
    }
}
