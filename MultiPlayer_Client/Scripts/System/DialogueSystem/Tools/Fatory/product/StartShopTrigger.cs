using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 打开商城事件
/// </summary>
public class StartShopTrigger : IEventTrigger
{
    public void Execute()
    {
        UIManager.Instance.ShowUI<ShopPanel>();
    }
}
