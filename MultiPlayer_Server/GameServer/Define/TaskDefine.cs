using System;
using System.Collections.Generic;
using Newtonsoft.Json; 
/// <summary>
/// 添加JsonProperty特性强制绑定与JSON文件的映射关系
/// 防止因命名大小写差异导致报错
/// </summary>
public enum TaskType
{
    [JsonProperty("MainLine")]
    MainLine = 0,  // 主线任务
    [JsonProperty("SideLine")]
    SideLine = 1,  // 支线任务
    [JsonProperty("Daily")]
    Daily = 2      // 日常任务
}
/// <summary>
/// 任务状态
/// </summary>
public enum TaskState
{
    [JsonProperty("InActive")]
    InActive=0,   //未激活
    [JsonProperty("InProgress")]
    InProgress =1, //进行中
    [JsonProperty("Completed")]
    Completed =2,  //已完成未领取奖励   
    [JsonProperty("Finished")]
    Finished =3,    //已完成
    [JsonProperty("Claimed")]
    Claimed =4
}
public enum TaskProgressType
{
    [JsonProperty("KillMonster")]
    KillMonster = 0, // 杀怪进度
    [JsonProperty("CollectItem")]
    CollectItem = 1, // 收集物品进度
    [JsonProperty("TalkNPC")]
    TalkNPC = 2      // NPC对话进度
}
public enum RewardType
{
    [JsonProperty("Gold")]
    Gold = 0, // 金币奖励
    [JsonProperty("Item")]
    Item = 1  // 道具奖励
}

public class TaskProgressDefine
{
    [JsonProperty("progressType")] // 与JSON字段名完全一致
    public TaskProgressType ProgressType { get; set; }

    [JsonProperty("targetParam")]
    public int TargetParam { get; set; }

    [JsonProperty("targetValue")]
    public int TargetValue { get; set; }

    [JsonProperty("progressDesc")]
    public string ProgressDesc { get; set; } = string.Empty; // 初始化空字符串，避免null
}

public class TaskRewardDefine
{
    [JsonProperty("rewardType")]
    public RewardType RewardType { get; set; }

    [JsonProperty("rewardId")]
    public int RewardId { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty; // 初始化空字符串
}

// 任务配置主类（对应JSON数组中的单个任务对象）
public class TaskDefine
{
    [JsonProperty("taskId")]
    public int TaskId { get; set; }

    [JsonProperty("taskType")]
    public TaskType TaskType { get; set; }

    [JsonProperty("taskState")]
    public TaskState TaskState { get; set; }

    [JsonProperty("taskName")]
    public string TaskName { get; set; } = string.Empty;

    [JsonProperty("taskDesc")]
    public string TaskDesc { get; set; } = string.Empty;

    [JsonProperty("canAbandon")]
    public bool CanAbandon { get; set; }

    [JsonProperty("progress")]
    public TaskProgressDefine ProgressTargets { get; set; } = new TaskProgressDefine(); // 初始化空列表，避免null

    [JsonProperty("rewards")]
    public TaskRewardDefine Rewards { get; set; } = new TaskRewardDefine(); // 初始化空列表，避免null

    [JsonProperty("sortOrder")]
    public int SortOrder { get; set; } = 0; // 默认排序权重0
}