using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
/// <summary>
/// JSON≈‰÷√”≥…‰¿‡
/// </summary>
[Serializable]
public class ShopDefine
{
    [JsonProperty("ID")]
    public int id;

    [JsonProperty("Name")]
    public string name;

    [JsonProperty("ShopType")]
    public string ShopType;

    [JsonProperty("Description")]
    public string Description;

    [JsonProperty("Price")]
    public int Price;

    [JsonProperty("IconPath")]
    public string IconPath;

    [JsonProperty("IsPurchased")]
    public bool IsPurchased = false;

    [JsonProperty("IsLimitPurchase")]
    public bool IsLimitPurchase;

    [JsonProperty("Count")]
    public int Count;

    [JsonProperty("Attack")]
    public float Attack;

    [JsonProperty("Defense")]
    public float Defense;

    [JsonProperty("RoleName")]
    public string RoleName;
}
