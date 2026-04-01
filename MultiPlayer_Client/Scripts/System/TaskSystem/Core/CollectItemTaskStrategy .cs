using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 收集策略
/// </summary>
public class CollectItemTaskStrategy : ITaskStrategy
{
    public int CalculateProgress(PlayerTaskData playerTask, TaskConfig taskConfig, object eventData)
    {
        // eventData为收集事件数据：(物品ID, 收集数量)
        var (itemId, collectCount) = ((int, int))eventData;

        //Id错误则返回当前进度
        if (itemId != taskConfig.Data.progress.targetParam)
            return playerTask.progress.currentValue;

        //返回不超过目标值的新的进度
        var newProgress = playerTask.progress.currentValue + collectCount;
        return Math.Min(newProgress, taskConfig.Data.progress.targetValue);
    }

    public void GrantReward(int playerId, TaskConfig taskConfig)
    {
        // todo
        //添加物品到背包或仓库

        Debug.Log($"玩家{playerId}完成收集任务{taskConfig.Data.taskId}，" +
            $"获得{taskConfig.Data.rewards.count}个{taskConfig.Data.rewards.description}");
    }

    public bool IsCompleted(PlayerTaskData playerTask, TaskConfig taskConfig)
    {
        return playerTask.progress.currentValue >= taskConfig.Data.progress.targetValue;
    }
}