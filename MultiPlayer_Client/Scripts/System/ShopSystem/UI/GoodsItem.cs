using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoodsItem : MonoBehaviour,IItemBase<BaseGoods>
{
    [Header("UI组件")]
    public Image IconImg;
    public Text NameText;
    public Button BuyBtn;
    public Text BuyBtnText;

    private BaseGoods _currentGoods;  
    /// <summary>
    /// 更新按钮状态
    /// </summary>
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
    /// <summary>
    /// 点击购买事件
    /// </summary>
    private void OnBuyBtnClick()
    {
        ShopManager.Instance.BuyGoodsRequest(_currentGoods.Id);
    }
    /// <summary>
    /// 清空数据
    /// </summary>
    public void ResetData()
    {
        _currentGoods = null;
        IconImg.sprite = null;
        NameText.text = "";
        BuyBtn.onClick.RemoveAllListeners();
        GetComponent<Button>()?.onClick.RemoveAllListeners();
    }
    /// <summary>
    /// 实现初始化数据接口
    /// </summary>
    /// <param name="item"></param>
    public async void InitInfo(BaseGoods item)
    {
        _currentGoods = item;
        //设置基础信息
        NameText.text = item.Name;
        IconImg.sprite = await AddressableManager.Instance.GetIconByIdAsync(item.Id);
        //更新购买按钮状态

        //绑定事件
        BuyBtn.onClick.RemoveAllListeners();
        BuyBtn.onClick.AddListener(OnBuyBtnClick);
        GetComponent<Button>().onClick.AddListener(() =>
        {
            //显示商品信息详情
            UIManager.Instance.Find<ShopPanel>().ShowGoodsDetail(item);
            //UpdateBtnState();
        });
    }
}
