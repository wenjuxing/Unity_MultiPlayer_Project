using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// 任务策略接口
/// </summary>
public interface ITaskStrategy
{
    /// <summary>
    /// 计算任务进度(杀敌 收集 对话)
    /// </summary>
    /// <param name="playerTask"></param>
    /// <param name="taskConfig"></param>
    /// <param name="eventData">事件数据(杀敌数量 Id)</param>
    /// <returns></returns>
    int CalculateProgress(PlayerTaskData playerTask,TaskConfig taskConfig,object eventData);
    /// <summary>
    /// 判断任务是否完成
    /// </summary>
    /// <param name="playerTask"></param>
    /// <param name="taskConfig"></param>
    /// <returns></returns>
    bool IsCompleted(PlayerTaskData playerTask,TaskConfig taskConfig);
    /// <summary>
    /// 发放奖励
    /// </summary>
    void GrantReward(int playerId,TaskConfig taskConfig);
}
