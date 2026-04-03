
#### 商店面板创建和显示商品
---
- 商品面板主要通过UIManager进行创建，创建后再次点击面板调用Base.Show()方法显示避免重复创建；商店面板主要由ScrollView滚动视图和商品Item组成，商品面板只需要滚动视图容量的商品Item，通过对象池进行创建和回收，避免频繁的创建和销毁，当面板向上滚动的时候超出视图范围的商品Item会被回收，重新进入视图范围的会从对象池中拿出来；
---
#### 商品Item类
---
- 商品Item是显示商品的基础容器，包含商品图标、名称、价格、商品购买按钮；实现了IItemBase的接口，通过Init初始化方法设置商品的信息、加载商品图标、绑定购买按钮回调事件;
---
##### 商品内容刷新方法(UpdateBtnState)
---
- 当购买商品后回调该方法，刷新商品信息；

```csharp
public void UpdateBtnState()
    {
        if (_currentGoods.IsPurchased&&_currentGoods.IsLimitPurchase)
        {
            BuyBtn.interactable = false;
            BuyBtnText.text = "已购买";
        }
        else
        {
            BuyBtn.interactable = true;
            BuyBtnText.text = $"购买";
        }
    }
```
---
#### 商店功能的实现
---
- 商品面板上方会有物品分类栏，点击不同的物品类型显示不同商品；
##### 商品类型列表创建
- 通过水平布局组规范商品类型Item的显示，从对象池中预加载商品类型Item预制件，遍历商品类型创建对应的类型Item；
```csharp
private void CreateTypeTab()
    {
        //清空原有标签
        foreach (Transform child in typeTabGroup)
        {
            if (child!=typeTabGroup.transform)
            {
                Destroy(child.gameObject);
            }
        }

        //创建标签
        List<GoodsType> types = ShopDataManager.Instance.GetAllGoodsTypes();
        foreach (var type in types)
        {
           var tab=ObjectPoolsManager.Instance.Spawn(typeTabPrefab,Vector3.one,Quaternion.identity, typeTabGroup.transform);
            tab.GetComponentInChildren<Text>().text = type.ToString();
            tab.GetComponent<Button>().onClick.AddListener(()=>SelectTypeTabs(type));
        }
    }
```
---
##### 点击商品类型标签显示对应商品
- 点击商品类型标签后会回调点击事件并传入当前标签的ItemType，然后刷新商品列表；
```csharp
    private void SelectTypeTabs(GoodsType goodsType)
    {
        _currentType = goodsType;
        //刷新商品列表
        RefreshGoodsList(goodsType);
    }
```
---
- 刷新商品列表：先回收掉原来的商品Item,再通过类型从商品数据管理器(ShopDataManager)中获取对应类型的商品列表，然后再创建对应的商品Item;

```csharp
private void RefreshGoodsList(GoodsType type)
    {
        //回收所有商品
        _currentShowItems.Clear();

        //获取对应类型的商品
        List<BaseGoods> goods = ShopDataManager.Instance.GetGoodsByType(type);
        Debug.Log("列表数量"+goods.Count);
        if (goods.Count==0)
        {
            Debug.LogError($"类型{type}:下无商品");
            return;
        }
        SV.RecycleItem();
        SV.items = goods;
        SV.CheckItemShowOrHide();
        // 重置滚动位置
        goodsScrollView.verticalNormalizedPosition = 1;
    }
```