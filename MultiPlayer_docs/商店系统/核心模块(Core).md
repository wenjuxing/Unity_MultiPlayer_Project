#### 商品管理器(ShopManager)
---
- 商品管理器是连接视图层和数据层的中间层级，处理购买点击后向服务器发送购买请求和处理购买响应并通知视图层级更新显示；
---
-  核心属性：购买逻辑锁，当玩家点击后逻辑锁会设为true，如果此时玩家再次点击购买就会弹窗提示并返回，只有当本次购买结束才会把购买逻辑锁设为false;
```csharp
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
        //购买中......
        //购买完毕
         _isBuying = false;
```
---
##### 商品购买请求(BuyGoodsRequest)
---
1. 先通过商品Id从商品数据管理(ShopDataManager)中获取商品信息，判断商品是否存在；
2. 再判断商品是否可以重复购买和货币是否足够；
3. 最后再向服务器发起购买请求；
```csharp
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
```
---
##### 购买商品响应

1. 先判断PlayerId是否正确，主要是为了验证购买者是否为自己；
2. 然后通过UIManager显示购买提示，并通过事件系统执行回调事件(和其他系统交互，例如购买成功后背包需要新增物品、任务系统可能会增加任务进度);
```csharp
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
```