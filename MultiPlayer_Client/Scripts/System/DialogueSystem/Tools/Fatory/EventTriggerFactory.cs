using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 事件触发工厂 (扩展新事件只需要加实现类)
/// </summary>
public static class EventTriggerFactory 
{
    /// <summary>
    /// 创建触发事件
    /// </summary>
    /// <param name="eventStr"></param>
    /// <returns></returns>
    public static IEventTrigger Create(string eventStr)
    {
        //分割
       string[] parts= eventStr.Split(',');
        if (parts.Length < 2) return null;

        return parts[0] switch
        {
            "StartTask" => new StartTaskTrigger(parts[1]),
            "GiveItem" => new GiveItemTrigger(parts[1]),
            "StartDialogue"=>new StartDialogueTrigger(parts[1]),
            "StartShop"=>new StartShopTrigger(),
            _ => null //未知事件类型 新增事件在此拓展
        };
    }
}
/// <summary>
/// 事件触发接口
/// </summary>
public interface IEventTrigger { void Execute(); }
