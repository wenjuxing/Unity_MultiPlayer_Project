using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Assets.Scripts.U3d_scripts;

public class ShopPanel : UIBase
{
    [Header("商店UI")]
    private TextMeshProUGUI currencyText;
    private Image goldIcon;
    private Transform typeTabGroup; //商品类型组
    private Transform shopItemContent;
    private GameObject typeTabPrefab;   //商品类型按钮预制件
    private Button typeTabBtn;   //商品类型按钮
    private ScrollRect goodsScrollView;
    private RectTransform content;
    private int ViewPortH = 800;
    private Transform goodsContents; //商品内容
    private GameObject goodsItemPrefab;
    public TextMeshProUGUI DetailNameText;
    public TextMeshProUGUI DetailPriceText;
    public TextMeshProUGUI DetailTypeText;
    public TextMeshProUGUI DetailDescriptionText;

    private List<GoodsItem> _currentShowItems = new List<GoodsItem>(); //当前显示的商品列表
    private GoodsType _currentType; //当前显示商品类型
    private BaseGoods _currentDetailGoods; //当前显示商品
    private BaseScrollView<BaseGoods, GoodsItem> SV;

    private void Awake()
    {
        //获取UI组件
        currencyText = transform.Find("Panel/Gold/CurrencyText").GetComponent<TextMeshProUGUI>();
        goldIcon = transform.Find("Panel/Gold/Icon").GetComponent<Image>();
        typeTabGroup = GameObject.FindWithTag("TypeTabGroup").transform;
        shopItemContent = GameObject.FindWithTag("ShopItemContent").transform;
        typeTabPrefab = Resources.Load("Prefabs/UI/ShopTypeItem") as GameObject;
        typeTabBtn = typeTabPrefab.GetComponent<Button>();
        goodsScrollView = transform.Find("Panel/ShopItemList").GetComponent<ScrollRect>();
        content = transform.Find("Panel/ShopItemList/Viewport/Content").GetComponent<RectTransform>();
        goodsItemPrefab = (GameObject)Resources.Load("Prefabs/UI/ShopItem");
        DetailNameText = transform.Find("ShopDetailPanel/DetailNameText").GetComponent<TextMeshProUGUI>();
        DetailPriceText = transform.Find("ShopDetailPanel/DetailPriceText").GetComponent<TextMeshProUGUI>();
        DetailTypeText = transform.Find("ShopDetailPanel/DetailTypeText").GetComponent<TextMeshProUGUI>();
        DetailDescriptionText = transform.Find("ShopDetailPanel/DetailDescriptionText").GetComponent<TextMeshProUGUI>();
        ViewPortH = (int)goodsScrollView.GetComponent<RectTransform>().rect.height;
        

        //初始化对象池
        ObjectPoolsManager.Instance.PreLoadPrefab(typeTabPrefab,5);

        //添加事件
        OnBtnClick("Panel/returnBtn",OnReturnBtn);
    }
    private void Start()
    {
        //200 220
        SV = new BaseScrollView<BaseGoods, GoodsItem>(200, 220, 5, "Prefabs/UI/ShopItem",
            content, ViewPortH, ShopDataManager.Instance._goodsList);
        OnScrollViewChanged("Panel/ShopItemList", () => { SV.CheckItemShowOrHide(); });
        InitShopUI();
    }
    private void OnEnable()
    {
        SV = new BaseScrollView<BaseGoods, GoodsItem>(200, 220, 5, "Prefabs/UI/ShopItem",
            content, ViewPortH, ShopDataManager.Instance._goodsList);
        //默认选择第一个类型
        List<GoodsType> types = ShopDataManager.Instance.GetAllGoodsTypes();
        if (types.Count > 0)
        {
            SelectTypeTabs(types[0]);
        }
        //更新货币显示
        UpdateCurrencyText();
    }
    private void OnDisable()
    {
        SV.RecycleItem();
    }
    /// <summary>
    /// 初始化商店UI
    /// </summary>
    private void InitShopUI()
    {
        //创建类型标签
        CreateTypeTab();
        //默认选择第一个类型
        List<GoodsType> types = ShopDataManager.Instance.GetAllGoodsTypes();
        if (types.Count>0)
        {
            SelectTypeTabs(types[0]);
        }
        //更新货币显示
        UpdateCurrencyText();
    }
    /// <summary>
    /// 选择类型标签
    /// </summary>
    /// <param name="goodsType"></param>
    private void SelectTypeTabs(GoodsType goodsType)
    {
        _currentType = goodsType;
        //刷新商品列表
        RefreshGoodsList(goodsType);
    }
    /// <summary>
    /// 创建类型标签
    /// </summary>
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
    /// <summary>
    /// 更新货币显示
    /// </summary>
    public void UpdateCurrencyText()
    {
        currencyText.text = GameApp.currency.ToString();
    }
    /// <summary>
    /// 刷新商品列表
    /// </summary>
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
    /// <summary>
    /// 按钮回调事件
    /// </summary>
    private void OnReturnBtn()
    {
        //回收Item到对象池
        SV.RecycleItem();
        base.Hide();
    }
    /// <summary>
    /// 显示商品细节
    /// </summary>
    public void ShowGoodsDetail(BaseGoods goods)
    {
        _currentDetailGoods = goods;
        DetailNameText.text = $"商品名称：{_currentDetailGoods.Name}";
        DetailPriceText.text = $"商品价格：{_currentDetailGoods.Price}";
        DetailTypeText.text = $"商品类型：{_currentDetailGoods.Type}";
        DetailDescriptionText.text = $"商品描述：暂时没有";
    }
}
