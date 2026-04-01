using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 杀敌策略
/// </summary>
public class KillMonsterTaskStrategy : ITaskStrategy
{
    public int CalculateProgress(PlayerTaskData playerTask, TaskConfig taskConfig, object eventData)
    {
        //元组解构
        var (monsterId, killCount) = ((int, int))eventData;

        //Id错误则返回当前进度
        if (monsterId != taskConfig.Data.progress.targetParam) return playerTask.progress.currentValue;

        //计算新的值
        int NewProgress = playerTask.progress.currentValue + killCount;

        //返回不超过目标值的新的进度
        return Math.Min(NewProgress,taskConfig.Data.progress.targetValue);
    }
   
    public void GrantReward(int playerId, TaskConfig taskConfig)
    {
        // todo
        //添加物品到背包或仓库

        Debug.Log($"玩家{playerId}完成杀怪任务{taskConfig.Data.taskId}，" +
            $"获得{taskConfig.Data.rewards.count}个{taskConfig.Data.rewards.description}");
    }
   
    public bool IsCompleted(PlayerTaskData playerTask, TaskConfig taskConfig)
    {
        return playerTask.progress.currentValue >= taskConfig.Data.progress.targetValue;
    }
}
