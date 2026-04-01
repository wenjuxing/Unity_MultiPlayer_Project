using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TaskPanel : UIBase
{
    //存放任务Item的字典
    private Dictionary<int, TaskItem> _taskItemDict = new Dictionary<int, TaskItem>();
    private Transform Parent;
    private TextMeshProUGUI TaskDescription;
    private void Start()
    {
        Parent = GameObject.FindWithTag("Content").transform;
        TaskDescription = GameObject.FindWithTag("TaskDescription").GetComponent<TextMeshProUGUI>();
        //注册事件
        TaskItem.OnClaimReward += OnClaimRewardHandler;
        TaskItem.OnAbandonTask += OnAbandonTaskHandler;
        TaskItem.OnAcceptTask += OnAcceptTaskHandler;
        TaskItem.OnClick += OnShowTaskDetail;
    }
    private void OnDestroy()
    {
        TaskItem.OnClaimReward -= OnClaimRewardHandler;
        TaskItem.OnAbandonTask -= OnAbandonTaskHandler;
        TaskItem.OnAcceptTask -= OnAcceptTaskHandler;
        TaskItem.OnClick -= OnShowTaskDetail;
    }

    private void OnAcceptTaskHandler(int taskId)
    {
        TaskManager.Instance.AcceptTaskRequest(taskId);
    }
    private void OnAbandonTaskHandler(int taskId)
    {
        TaskManager.Instance.AbanbonTaskRequest(taskId);
    }
    private void OnClaimRewardHandler(int taskId)
    {
        TaskManager.Instance.ClaimRewardRequest(taskId);
    }

    /// <summary>
    /// 加载任务更新UI
    /// </summary>
    /// <param name="playerTask"></param>
    public void OnTaskLoading(PlayerTaskData playerTask)
    {
        TaskConfig config = TaskConfigManager.Instance.GetTaskConfig(playerTask.taskId);
        if (config == null) return;

        //异步创建任务条
        StartCoroutine(CreateTaskItem(playerTask, config));
    }
    /// <summary>
    /// 接取任务时更新UI
    /// </summary>
    public void OnAcceptTask(int taskId)
    {
        Debug.Log("接取任务时更新UI");

        if (_taskItemDict.TryGetValue(taskId,out var taskItem))
        {
            taskItem.UpdateState(TaskState.InProgress);
        }
    }
    /// <summary>
    /// 任务进度更新时更新UI
    /// </summary>
    /// <param name="playerTask"></param>
    public void OnTaskProgressUpdated(PlayerTaskData playerTask)
    {
        if (_taskItemDict.TryGetValue(playerTask.taskId,out TaskItem taskItem))
        {
            taskItem.UpdateProgress(playerTask);
        }
    }
    /// <summary>
    /// 任务完成时更新UI
    /// </summary>
    /// <param name="playerTask"></param>
    public void OnTaskCompleted(PlayerTaskData playerTask)
    {
        if (_taskItemDict.TryGetValue(playerTask.taskId, out TaskItem taskItem))
        {
            taskItem.UpdateState(playerTask.taskState);
        }
    }
    /// <summary>
    /// 领取奖励后更新UI
    /// </summary>
    public void OnRewardClaimed(int taskId)
    {
        if (_taskItemDict.TryGetValue(taskId,out TaskItem taskItem))
        {
            taskItem.UpdateState(TaskState.Finished);
        }
    }
    /// <summary>
    /// 放弃任务重置UI
    /// </summary>
    public void OnTaskAbandoned(int taskId)
    {
        if (_taskItemDict.TryGetValue(taskId, out TaskItem taskItem))
        {
            ObjectPoolsManager.Instance.Despawn(taskItem.gameObject,0);
            _taskItemDict.Remove(taskId);
        }
    }
    /// <summary>
    /// 显示任务详情
    /// </summary>
    /// <param name="playerTask"></param>
    public void OnShowTaskDetail(PlayerTaskData playerTask)
    {
        TaskConfig config = TaskConfigManager.Instance.GetTaskConfig(playerTask.taskId);
        if (config == null) return;

        //更新文本显示
        TaskDescription.text = config.Data.taskDesc;
    }
    /// <summary>
    /// 创建任务条
    /// </summary>
    /// <param name="playerTask"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    IEnumerator CreateTaskItem(PlayerTaskData playerTask,TaskConfig config)
    {
        var res = Resources.LoadAsync<GameObject>("Prefabs/Task/TaskItem");
        yield return res;
        var item = ObjectPoolsManager.Instance.Spawn(res.asset as GameObject,Vector3.zero,Quaternion.identity, Parent);
        TaskItem taskItem = item.GetComponent<TaskItem>();
        taskItem.Init(playerTask, config);
        _taskItemDict.Add(playerTask.taskId,taskItem);
    }
}
