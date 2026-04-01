using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    /// <summary>
    /// JSON配置映射类
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

