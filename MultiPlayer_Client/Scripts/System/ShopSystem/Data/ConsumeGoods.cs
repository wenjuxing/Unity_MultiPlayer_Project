using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConsumeGoods:BaseGoods
{
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
