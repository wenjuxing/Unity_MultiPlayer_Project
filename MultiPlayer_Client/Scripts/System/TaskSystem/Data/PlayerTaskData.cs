using System;
using System.Collections.Generic;

/// <summary>
/// 玩家单个任务的动态数据
/// 对应一个任务的当前状态、进度、奖励领取情况
/// </summary>
[Serializable] // 必须加，否则无法JSON序列化
public class PlayerTaskData
{
    /// <summary>
    /// 任务ID（关联TaskConfig的唯一标识）
    /// </summary>
    public int taskId;

    /// <summary>
    /// 当前任务状态（未激活/进行中/完成未领取/已领取）
    /// </summary>
    public TaskState taskState;

    /// <summary>
    /// 任务进度列表（对应TaskConfig的多个进度目标）
    /// </summary>
    public PlayerTaskProgress progress;

    /// <summary>
    /// 奖励是否已领取（避免重复领取）
    /// </summary>
    public bool isRewardClaimed;

    /// <summary>
    /// 构造函数（初始化默认值）
    /// </summary>
    public PlayerTaskData()
    {
        taskState = TaskState.InActive;
        progress = new PlayerTaskProgress();
        isRewardClaimed = false;
    }
}

/// <summary>
/// 玩家单个任务进度的动态数据
/// 对应TaskConfig中的一个进度目标（如“击杀5只狼”的当前击杀数）
/// </summary>
[Serializable]
public class PlayerTaskProgress
{
    /// <summary>
    /// 进度类型（击杀/收集/对话等）
    /// </summary>
    public TaskProgressType progressType;

    /// <summary>
    /// 目标参数（怪物ID/道具ID/区域ID等
    /// </summary>
    public int targetParam;

    /// <summary>
    /// 当前进度值（如已击杀3只狼）
    /// </summary>
    public int currentValue;
}