- Excel表格：存储商品数据，包含商品Id、商品名字、商品类型等；通过Windows的cmd命令把Excel表格的数据转为Json文件；
![[Pasted image 20260402160934.png]]
---
- Json文件(双端持有)：使用商品ID作为键，商品其他数据作为键的存储格式，可以通过类进行映射然后存储到字典当中；
```json
"1001": {

    "ID": 1001,

    "Name": "内测徽章",

    "ShopType": "道具",

    "Description": "        属于内测勇士的专属徽章",

    "Price": 100,

    "IconPath": "Assets/Sprites/GoodsIcons/1001",

    "IsPurchased": false,

    "Count": 10,

    "Attack": 0,

    "Defense": 0,

    "RoleName": "",

    "IsLimitPurchase": false

  },
```
---
- 映射类：用于映射商品Json文件，然后在通过数据管理器存储在字典当中；
```csharp
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
```
---
- DataManager（双端）：数据管理器通过加载Json文件映射到类中，并把映射类存储到字典中；
```csharp
public Dictionary<int, T> Load<T>(string filePath)
    {
        // 获取exe文件所在目录的绝对路径
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exePath);
        // 构建1.txt文件的完整路径
        string txtFilePath = Path.Combine(exeDirectory, filePath);
        // 读取1.txt文件的内容
        string content = File.ReadAllText(txtFilePath);
        // 打印1.txt文件的内容
        //Console.WriteLine(content);
        return JsonConvert.DeserializeObject<Dictionary<int, T>>(content, settings);
    }
```
---
- 客户端商品抽象基类：定义商品的基础属性(Id、Name、Type、Price等)和商品基础方法(Buy()、GetInfo());
```csharp
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
```
---
- 客户端商品具体类：定义商品类的具体属性和实现基类抽象方法；
```csharp
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
}
```
---
- 客户端ShopDataManager：按照商品的分类存储在对应的字典当中，遍历所有的商品数据，然后创建对应的商品类，赋值并存储到对应的字典当中方便查询；
```csharp
//商品字典
public Dictionary<int, PropGoods> _propGoodsDict = new Dictionary<int, PropGoods>();
    public Dictionary<int, EquipGoods> _equipGoodsDict = new Dictionary<int, EquipGoods>();
    public Dictionary<int, SkinGoods> _skinGoodsDict = new Dictionary<int, SkinGoods>();
    public Dictionary<int, ConsumeGoods> _consumeGoodsDict = new Dictionary<int, ConsumeGoods>();
//商品分类字典赋值....
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
```