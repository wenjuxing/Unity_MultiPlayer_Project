using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TaskEventCenter
{
    /// <summary>
    /// 事件定义
    /// </summary>
    public static event Action<int,KillMonsterEventArgs> OnKillMonster;
    public static event Action<int,CollectItemEventArgs> OnCollectItem;
    public static event Action<int,LevelUpEventArgs> OnLevelUp;
    public static event Action<int,int> OnTalkNPC;

    /// <summary>
    /// 事件触发方法(外部系统调用)
    /// </summary>
    /// <param name="args"></param>
    public static void TriggerKillMonster(int playerId,KillMonsterEventArgs args)
    {
        OnKillMonster?.Invoke(playerId,args);
    }
    public static void TriggerCollectItem(int playerId, CollectItemEventArgs args)
    {
        OnCollectItem?.Invoke(playerId,args);
    }
    public static void TriggerLevelUp(int playerId, LevelUpEventArgs args)
    {
        OnLevelUp?.Invoke(playerId,args);
    }
    public static void TriggerTalkNPC(int playerId,int Id)
    {
        OnTalkNPC?.Invoke(playerId,Id);
    }
}
