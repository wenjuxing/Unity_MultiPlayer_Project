using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
using Assets.Scripts.U3d_scripts;

public class ShopManager : SingletonBase<ShopManager>
{
    private bool _isBuying=false;//购买逻辑锁
    private void Awake()
    {
        MessageRouter.Instance.Subscribe<BuyGoodsResponse>(_BuyGoodsResponse);
    }

    /// <summary>
    /// 购买请求
    /// </summary>
    public void BuyGoodsRequest(int goodsId)
    {
        //防止重复点击
        if (_isBuying)
        {
            (UIManager.Instance.ShowUI<TipType1Panel>() as TipType1Panel).Show("通知", "请勿重复点击!", () =>
            {
                UIManager.Instance.HideUI("TipType1Panel");
            });
            return;
        }
        _isBuying = true;
        try 
        {
            //获取商品
           BaseGoods goods= ShopDataManager.Instance._goodsDict[goodsId];

            //判断商品是否存在
            if (goods==null)
            {
                (UIManager.Instance.ShowUI<TipType1Panel>() as TipType1Panel).Show("通知", "商品不存在!", () =>
                {
                    UIManager.Instance.HideUI("TipType1Panel");
                });
                return;
            }
            //判断商品是否可以重复购买
            if (goods.IsPurchased&&goods.IsLimitPurchase)
            {
                (UIManager.Instance.ShowUI<TipType1Panel>() as TipType1Panel).Show("通知", "本商品限购一个!", () =>
                {
                    UIManager.Instance.HideUI("TipType1Panel");
                });
                return;
            }
            //判断货币是否足够
            if (GameApp.currency<goods.Price)
            {
                (UIManager.Instance.ShowUI<TipType1Panel>() as TipType1Panel).Show("通知", "余额不足!", () =>
                {
                    UIManager.Instance.HideUI("TipType1Panel");
                });
                return;
            }
            //发送购买请求信息
            BuyGoodsRequest res = new BuyGoodsRequest();
            res.PlayerId = GameApp.playerId;
            res.GoodsId = goodsId;
            NetClient.Send(res);
           
        }
        catch(Exception e)
        {
            Debug.LogError($"购买异常{e.Message}");
            return;
        }
        finally
        {
            //释放锁
            _isBuying = false;
        }
    }
    /// <summary>
    /// 购买响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _BuyGoodsResponse(Connection conn, BuyGoodsResponse msg)
    {
        //合法性检验
        if (GameApp.playerId==msg.PlayerId)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (msg.Success)
                {
                    Debug.Log($"购买成功！{msg.StorageInfo.GoodsList.Count}");
                    (UIManager.Instance.ShowUI<TipType1Panel>()as TipType1Panel).Show("通知","购买成功!",()=>
                    {
                        UIManager.Instance.HideUI("TipType1Panel");
                        TaskEventCenter.TriggerCollectItem(GameApp.playerId, new CollectItemEventArgs(msg.GoodsId,1));
                        Kaiyun.Event.FireIn("AddItemRequest",msg.GoodsId,1);
                    });
                }
                else
                {
                    Debug.Log(msg.ErrorMsg);
                    (UIManager.Instance.ShowUI<TipType1Panel>() as TipType1Panel).Show("通知", $"{msg.ErrorMsg}", () =>
                    {
                        UIManager.Instance.HideUI("TipType1Panel");
                    });
                    return;
                }
            });
        }
    }
}
