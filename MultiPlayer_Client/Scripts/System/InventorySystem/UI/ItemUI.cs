using GameClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void ShowItemText(string content);
public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [Header("组件")]
    public Text AmountText;
    public Image IconImage;
    [Header("属性")]
    private Vector3 offset;           //偏移量
    private Vector3 InitPosition;     //初始位置
    private Transform InitTransform;  //初始父对象
    private UISlot InitSlot;          //初始插槽
    private bool IsDragging;          //正在拖拽？ 
    public RectTransform rectTransform;
    public Item Item { get; set; }
    public static event ShowItemText showItemText;
    private void Start()
    {
        AmountText = GetComponentInChildren<Text>();
        IconImage = GetComponentInChildren<Image>();
        AmountText.raycastTarget = false;
    }
    /// <summary>
    /// 确定事件
    /// </summary>
    /// <param name="value"></param>
    private void OnOKBtn(int value)
    {
        Kaiyun.Event.FireIn("ItemDiscard", InitSlot.InitIndex, value);
        NumInputPanel.onOKBtnClick -= OnOKBtn;
    }
    /// <summary>
    /// 取消事件
    /// </summary>
    private void OnCanelBtn()
    {
        Kaiyun.Event.FireIn("ItemDiscard", InitSlot.InitIndex, 0);
        NumInputPanel.onCanelBtnClick -= OnCanelBtn;
    }

    private void Update()
    {
        if (Item != null)
        {
            //设置物品数量
            AmountText.text = Item.amount.ToString();
            AmountText.gameObject.SetActive(Item.amount > 1);
            //设置物品图标
            IconImage.sprite = Item.SpriteImage;
        }
    }
    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        //设置初始插槽 位置 父对象 
        InitSlot = transform.parent.GetComponent<UISlot>();
        InitPosition = transform.position;
        InitTransform = transform.parent;
        //计算偏移量
        offset = transform.position - Input.mousePosition;
        //从原来的插槽移除
        transform.SetParent(transform.root);
        //设置开始拖拽属性
        IsDragging = true;
        IconImage.raycastTarget = false;
    }
    /// <summary>
    /// 正在拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition + offset;
    }
    /// <summary>
    /// 拖拽结束
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        UISlot targetSlot = null;
        //鼠标是否悬停在UI元素上面
        if (EventSystem.current.IsPointerOverGameObject())
        {
            //尝试获取当前悬停UI的UISlot组件
            targetSlot = eventData.pointerEnter.gameObject.GetComponent<UISlot>();
            if (targetSlot == null)
            {
                //可能悬停在图标上面
                targetSlot = eventData.pointerEnter.GetComponentInParent<UISlot>();
            }
            //放置物品
            if (targetSlot!=null)
            {
                //物品交换
                if (targetSlot.ItemUI==null)
                {
                    Debug.Log("将物品放置到目标格子");
                    targetSlot.ItemUI = this;
                    //因为重新加载服务器的物品，当前的物品可以直接销毁
                    Destroy(this.gameObject);
                    //放置物品
                    ItemPlacement(InitSlot.Index, targetSlot.Index);
                }
                else
                {
                    Debug.Log("物品交换");
                    //把目标插槽的图标设置到初始化的插槽
                    InitSlot.ItemUI = targetSlot.ItemUI;
                    //把被拖拽图标设置到目标插槽
                    targetSlot.ItemUI = this;
                    //因为重新加载服务器的物品，当前的物品可以直接销毁
                    Destroy(this.gameObject);
                    //放置物品
                    ItemPlacement(InitSlot.Index, targetSlot.Index);

                    //交换后重置原槽位物品UI的锚点
                    RectTransform rt = InitSlot.ItemUI.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                    }
                }
                RectTransform targetRt = targetSlot.ItemUI.GetComponent<RectTransform>();
                if (targetRt != null)
                {
                    targetRt.anchorMin = new Vector2(0.5f, 0.5f);
                    targetRt.anchorMax = new Vector2(0.5f, 0.5f);
                    targetRt.pivot = new Vector2(0.5f, 0.5f);
                    targetRt.anchoredPosition = Vector2.zero;
                }

            }
            else
            {
                //丢弃或者还原
                InitSlot.ItemUI = this;
                InitSlot.ItemUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                InitSlot.ItemUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                InitSlot.ItemUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                InitSlot.ItemUI.rectTransform.anchoredPosition = Vector2.zero;

            }
        }
        else
        {
            //销毁图标
            Destroy(this.gameObject);
            //丢弃物品
            //提示面板弹框
            UIManager.Instance.ShowUI<NumInputPanel>(E_UIPanelLayer.Forefront);
            //注册事件
            NumInputPanel.onCanelBtnClick += OnCanelBtn;
            NumInputPanel.onOKBtnClick += OnOKBtn;
        }
        IconImage.raycastTarget = true;
        IsDragging = false;
    }
    /// <summary>
    /// 触发放置物品事件
    /// </summary>
    /// <param name="OriginIndex"></param>
    /// <param name="TargetIndex"></param>
    private void ItemPlacement(int OriginIndex, int TargetIndex)
    {
        Kaiyun.Event.FireIn("ItemPlacement", OriginIndex,TargetIndex);
    }
    /// <summary>
    /// 鼠标进入事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //string content = "<color=#ffffff>物品信息为空</color>";
        //if (Item!=null)
        //{
        //    content = Item.GetTipText();
        //}
        //UIManager.Instance.ShowUI<ItemInfoPanel>(E_UIPanelLayer.Forefront);
        //showItemText?.Invoke(content);
    }
    /// <summary>
    /// 鼠标离开事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        //UIManager.Instance.HideUI("ItemInfoPanel");
    }
    /// <summary>
    /// 鼠标点击事件
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //双击使用物品
        if (eventData.clickCount == 2)
        {
            var slot = transform.parent.GetComponent<UISlot>();
            if (slot != null)
            {
                Kaiyun.Event.FireIn("UseItem", slot.Index);
            }
        }
    }
}
