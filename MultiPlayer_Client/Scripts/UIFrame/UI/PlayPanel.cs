using Assets.Scripts.U3d_scripts;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPanel : UIBase
{
    private void Awake()
    {
        OnToggleClick("Panel/Bag/Toggle", KnapsackToggle);
        OnToggleClick("Panel/Task/Toggle", TaskPanelToggle);
        OnToggleClick("Panel/Shop/Toggle", ShopPanelToggle);
    }
    /// <summary>
    /// 开启商店
    /// </summary>
    /// <param name="arg0"></param>
    private void ShopPanelToggle(bool isShow)
    {
        if (isShow)
        {
            UIManager.Instance.ShowUI<ShopPanel>();
        }
        else
        {
            UIManager.Instance.HideUI("ShopPanel");
        }
    }
    /// <summary>
    /// 开启任务列表
    /// </summary>
    private void TaskPanelToggle(bool isShow)
    {
        if (isShow)
        {
            UIManager.Instance.ShowUI<TaskPanel>();
            TaskManager.Instance.TaskDataRequest();
        }
        else
        {
            UIManager.Instance.HideUI("TaskPanel");
        }
    }
    /// <summary>
    /// 开关背包
    /// </summary>
    public void KnapsackToggle(bool isShow)
    {
        if (GameApp.character != null)
        {
            if (isShow)
            {
                UIManager.Instance.ShowUI<KnapsackPanel>(E_UIPanelLayer.Front);
                InventoryRequest resp = new InventoryRequest()
                {
                    EntityId = GameApp.character.entityId,
                    QueryKnapsack = true
                };
                NetClient.Send(resp);
            }
            else
            {
                UIManager.Instance.HideUI("KnapsackPanel");
            }
                
        }
    }
}
