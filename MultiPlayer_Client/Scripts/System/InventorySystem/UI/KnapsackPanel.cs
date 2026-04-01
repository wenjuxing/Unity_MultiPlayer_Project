using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using Assets.Scripts.U3d_scripts;
using GameClient;
using TMPro;
using UnityEngine.UI;
using Serilog;
using System;

public class KnapsackPanel : UIBase
{
    [Header("背包UI")]
    private TextMeshProUGUI currencyText;
    private Image goldIcon;
    private Transform typeTabGroup; //商品类型组
    private Transform GoodsItemContent;
    private GameObject typeTabPrefab;   //商品类型按钮预制件
    private Button typeTabBtn;   //商品类型按钮
    private ScrollRect goodsScrollView;
    private RectTransform content;
    private int ViewPortH = 800;
    private Transform goodsContents; //商品内容
    private GameObject goodsItemPrefab;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI QualityText;
    public TextMeshProUGUI DescriptionText;
    private GameObject UISlotPrefab;
    private Button returnBtn;
    //自定义滚动视图
    private InventoryScrollView SV;

    private ItemType currentType;
    private void Awake()
    {
        //获取UI组件
        currencyText = transform.Find("Panel/CurrencyPanel/Gold/CurrencyText").GetComponent<TextMeshProUGUI>();
        goldIcon = transform.Find("Panel/CurrencyPanel/Gold/Icon").GetComponent<Image>();
        typeTabGroup = GameObject.FindWithTag("TypeTabGroup").transform;
        typeTabPrefab = Resources.Load("Prefabs/UI/GoodsTypeItem") as GameObject;
        typeTabBtn = typeTabPrefab.GetComponent<Button>();
        goodsScrollView = transform.Find("Panel/GoodsList").GetComponent<ScrollRect>();
        content = transform.Find("Panel/GoodsList/Viewport/Content").GetComponent<RectTransform>();
        returnBtn = transform.Find("Panel/returnBtn").GetComponent<Button>();

        NameText = transform.Find("Panel/GoodsInfoPanel/Name").GetComponent<TextMeshProUGUI>();
        TypeText = transform.Find("Panel/GoodsInfoPanel/Type").GetComponent<TextMeshProUGUI>();
        QualityText = transform.Find("Panel/GoodsInfoPanel/Quality").GetComponent<TextMeshProUGUI>();
        DescriptionText = transform.Find("Panel/GoodsInfoPanel/Description").GetComponent<TextMeshProUGUI>();

        ViewPortH = (int)goodsScrollView.GetComponent<RectTransform>().rect.height;

        goodsItemPrefab = Resources.Load("Prefabs/UI/GoodsItem")as GameObject;
        UISlotPrefab = Resources.Load("Prefabs/UI/GoodsSlot")as GameObject;
        //初始化对象池
        ObjectPoolsManager.Instance.PreLoadPrefab(typeTabPrefab, 5);

        //注册事件
        Kaiyun.Event.RegisterIn("ShowItemDetails", this, "ShowItemDetails");
        returnBtn.onClick.AddListener(() => base.Hide());
    }
    private void Start()
    {      
        Kaiyun.Event.RegisterOut("OnKnapsackReloaded",this, "OnKnapsackReloaded");
        //销毁测试时创建的插槽
        foreach (var uiSlot in transform.GetComponentsInChildren<UISlot>())
        {
            Destroy(uiSlot.gameObject);
        }
        SV = new InventoryScrollView(122, 122, 6, "Prefabs/UI/GoodsSlot", content,ViewPortH, new List<Item>(), UISlotPrefab);
        OnScrollViewChanged("Panel/GoodsList", () => { SV.CheckItemShowOrHide(); });

        ResetItemDetails();
        InitItemTypeUI();
    }
    private void OnEnable()
    {
        UpdateCurrencyText();
    }
    /// <summary>
    /// 初始化商品类型UI
    /// </summary>
    public void InitItemTypeUI()
    {
        CreateTypeTab();
        SelectTypeTab(ItemType.All);
    }
    /// <summary>
    /// 加载背包信息
    /// </summary>
    public void OnKnapsackReloaded()
    {
        SV.RecycleItem();
        var chr = GameApp.character;
        var currentTypeItems = InventoryDataMgr.Instance.GetItemListByType(currentType);
        SV.items = currentTypeItems;

        SV.CheckItemShowOrHide(() =>
        {
            var slotList = transform.GetComponentsInChildren<UISlot>();
            for (int i = 0; i < slotList.Length; i++)
            {
                var slot = slotList[i];
                slot.Index = i;
                if (currentType==ItemType.All)
                {
                    slot.InitIndex = i;
                }
                Item targetItem = null;
                if (i < currentTypeItems.Count)
                {
                    targetItem = currentTypeItems[i];
                }
                slot.InitInfo(targetItem);
            }
        });
    }
    /// <summary>
    /// 显示商品详情
    /// </summary>
    public void ShowItemDetails(Item item)
    {
        ResetItemDetails();
        NameText.text = $"名称:{item.Name}";
        TypeText.text = $"类型:{item.ItemType}";
        QualityText.text = $"品质:{item.Quality}";
        DescriptionText.text = $"描述:{item.Description}";
    }
    /// <summary>
    /// 重置商品详情信息
    /// </summary>
    private void ResetItemDetails()
    {
        NameText.text = "";
        TypeText.text = "";
        QualityText.text = "";
        DescriptionText.text = "";
    }
    /// <summary>
    /// 创建商品类型标签
    /// </summary>
    private void CreateTypeTab()
    {
        //清空原有的标签
        foreach (Transform child in typeTabGroup)
        {
            if (child != typeTabGroup.transform)
            {
                ObjectPoolsManager.Instance.Despawn(typeTabPrefab,0);
            }
        }
        //创建新的标签
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
           GameObject tab=ObjectPoolsManager.Instance.
                Spawn(typeTabPrefab,Vector3.one,Quaternion.identity, typeTabGroup.transform);
            tab.GetComponentInChildren<TextMeshProUGUI>().text = type.ToString();
            tab.GetComponent<Button>().onClick.AddListener(() => 
            {
                SelectTypeTab(type);
            });
        }
    }
    /// <summary>
    /// 选择商品类型标签
    /// </summary>
    private void SelectTypeTab(ItemType type)
    {
        Debug.Log("类型" + type);
        currentType = type;
        OnKnapsackReloaded();
    }
    /// <summary>
    /// 更新货币显示
    /// </summary>
    public void UpdateCurrencyText()
    {
        currencyText.text = GameApp.currency.ToString();
    }
}
