using System.Collections.Generic;
using System;
/// <summary>
/// 任务类型
/// </summary>
public enum TaskType
{
    MainLine,  //主线
    SideLine,  //支线
    Daily      //日常
}
/// <summary>
/// 任务状态
/// </summary>
public enum TaskState
{
    InActive,   //未激活
    InProgress, //进行中
    Completed,  //已完成未领取奖励   
    Finished,    //已完成
    Claimed
}
/// <summary>
/// 任务进度类型
/// </summary>
public enum TaskProgressType
{
    KillMonster, //杀怪
    CollectItem, //收集物品
    TalkNPC      //NPC对话
}
/// <summary>
/// 任务奖励类型
/// </summary>
public enum RewardType
{
    Gold, //金币
    Item  //道具
}
[Serializable]
public class TaskProgressConfig
{
    public TaskProgressType progressType; // 进度类型
    public int targetParam;               // 目标参数（如怪物ID、道具ID、区域ID）
    public int targetValue;               // 目标值（如击杀5只、收集10个）
    public string progressDesc;           // 进度描述（UI显示用，如“击杀5只[狼]”）
}
[Serializable]
public class RewardConfig
{
    public RewardType rewardType; // 奖励类型
    public int rewardId;          // 道具ID（仅道具类型有效）
    public int count;             // 数量（金币/经验/道具数量）
    public string description;    // 奖励描述（UI显示用）
}
[Serializable]
public class TaskConfigData
{
    public int taskId;                  // 任务唯一ID（不可重复）
    public TaskType taskType;           // 任务类型
    public TaskState taskState;         // 任务状态
    public string taskName;             // 任务名称
    public string taskDesc;             // 任务描述
    public bool canAbandon;             // 是否可放弃
    public TaskProgressConfig progress; // 进度目标列表
    public RewardConfig rewards;  // 奖励列表
    public int sortOrder;               // UI显示排序权重（越大越靠前）
}
