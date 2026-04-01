using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public abstract class BaseGoods
{
    public int Id;
    public string Name;
    public int Price;
    public GoodsType Type;
    public string IconPath;
    public bool IsPurchased;
    public bool IsLimitPurchase;

    public abstract bool Buy();
    public abstract string GetExtInfo();
}
