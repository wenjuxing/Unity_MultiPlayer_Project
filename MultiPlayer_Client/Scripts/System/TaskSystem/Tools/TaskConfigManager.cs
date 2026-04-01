using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 任务配置表管理器
/// </summary>
public class TaskConfigManager : SingletonBase<TaskConfigManager>
{
    //任务配置字典
    public Dictionary<int, TaskConfig> _taskConfigDict = new Dictionary<int, TaskConfig>();

    /// <summary>
    /// 加载所有任务配置
    /// </summary>
    public void LoadAllTaskConfigs()
    {
        TaskConfig[] configs = Resources.LoadAll<TaskConfig>("TaskConfigs");
        foreach (var config in configs)
        {
            if (!_taskConfigDict.ContainsKey(config.Data.taskId))
            {
                _taskConfigDict.Add(config.Data.taskId, config);
            }
            else
            {
                Debug.LogWarning($"任务重复:TaskID{config.Data.taskId}");
            }
        }
    }
    /// <summary>
    /// 通过Id获取任务配置
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public TaskConfig GetTaskConfig(int taskId)
    {
        if (_taskConfigDict.TryGetValue(taskId, out TaskConfig task)) return task;
        else return null;
    }
    /// <summary>
    /// 获取所有任务配置
    /// </summary>
    /// <returns></returns>
    public List<TaskConfig> GetAllTaskConfigs(TaskType type=TaskType.MainLine)
    {
        List<TaskConfig> taskList = new List<TaskConfig>();
        foreach (var config in _taskConfigDict.Values)
        {
            if (config.Data.taskType==type)
            {
                taskList.Add(config);
            }
        }
        return taskList;
    }
}
