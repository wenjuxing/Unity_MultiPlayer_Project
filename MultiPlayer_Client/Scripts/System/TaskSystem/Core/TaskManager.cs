using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Proto;
using Summer.Network;

public class TaskManager : SingletonBase<TaskManager>
{
    //玩家所有任务信息
    private List<PlayerTaskData> _playerTaskDatas = new List<PlayerTaskData>();
    //任务配置字典
    private Dictionary<int, TaskConfig> _taskConfigDict => TaskConfigManager.Instance._taskConfigDict;
    private void Start()
    {
        //加载静态任务配置数据
        TaskConfigManager.Instance.LoadAllTaskConfigs();

        //订阅事件
        SubscribeEvents();

        //订阅响应事件
        MessageRouter.Instance.Subscribe<TaskDataResponse>(_TaskDataResponse);
        MessageRouter.Instance.Subscribe<AcceptTaskResponse>(_AcceptTaskResponse);
        MessageRouter.Instance.Subscribe<AbandonTaskResponse>(_AbandonTaskResponse);
        MessageRouter.Instance.Subscribe<TaskProUpdateResponse>(_TaskProUpdateResponse);
        MessageRouter.Instance.Subscribe<SubmitTaskResponse>(_SubmitTaskResponse);
    }
    /// <summary>
    /// 订阅任务事件
    /// </summary>
    private void SubscribeEvents()
    {
        TaskEventCenter.OnKillMonster += OnKillMonsterHandler;
        TaskEventCenter.OnCollectItem += OnCollectItemHandler;
        TaskEventCenter.OnTalkNPC += OnTalkNPCHandler;
    }
    /// <summary>
    /// 注销事件
    /// </summary>
    private void OnDestroy()
    {
        TaskEventCenter.OnKillMonster -= OnKillMonsterHandler;
        TaskEventCenter.OnCollectItem -= OnCollectItemHandler;
        TaskEventCenter.OnTalkNPC -= OnTalkNPCHandler;
    }

    #region 激活任务
    /// <summary>
    /// 接受任务请求
    /// </summary>
    /// <param name="taskId"></param>
    public void AcceptTaskRequest(int taskId)
    {
        //检查任务是否存在
        if (!_taskConfigDict.TryGetValue(taskId, out var config))
        {
            Debug.Log($"任务不存在,TaskId{taskId}");
            return;
        }
        //客户端先预测计算结果
        //修改任务状态
        var target = _playerTaskDatas.FirstOrDefault(t => t.taskId == taskId);
        if (target != null && target.taskState == TaskState.InActive)
        {
            target.taskState = TaskState.InProgress;
        }
        //通知UI更新
        UIManager.Instance.Find<TaskPanel>().OnAcceptTask(taskId);
        Debug.Log($"客户端预测:激活成功");

        //初始化玩家任务数据
        PlayerTaskData playerTask = new PlayerTaskData()
        {
            taskId = taskId,
            taskState = TaskState.InActive,
            isRewardClaimed = false,
            progress = new PlayerTaskProgress()
            {
                progressType = config.Data.progress.progressType,
                targetParam = config.Data.progress.targetParam,
                currentValue = 0
            }
        };
        //保存至服务器
        AcceptTaskRequest res = new AcceptTaskRequest()
        {
            TaskId = playerTask.taskId
        };
        NetClient.Send(res);
        Debug.Log($"任务接取成功:{config.name}");
    }
    /// <summary>
    /// 接取任务响应事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _AcceptTaskResponse(Connection conn, AcceptTaskResponse msg)
    {
        //接取任务成功？
        if (!msg.Success)
        {
            Debug.Log($"接取:{msg.ErrorMsg}");
            return;
        }
        //修改任务状态
        var target = _playerTaskDatas.FirstOrDefault(t => t.taskId == msg.TaskId);
        if (target != null && target.taskState == TaskState.InActive)
        {
            target.taskState = TaskState.InProgress;
        }
        //主线程中执行unity的程序
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //通知UI更新
            UIManager.Instance.Find<TaskPanel>().OnAcceptTask(msg.TaskId);
        });
    }
    #endregion

    #region 获取任务信息
    /// <summary>
    /// 任务信息请求
    /// </summary>
    public void TaskDataRequest()
    {
        TaskDataRequest res = new TaskDataRequest();
        NetClient.Send(res);
    }
    /// <summary>
    /// 任务信息响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _TaskDataResponse(Connection conn, TaskDataResponse msg)
    {
        foreach (var task in msg.TaskInfo)
        {
            Debug.Log($"任务状态:{task.TaskState}");
            //初始化玩家任务数据
            PlayerTaskData playerTask = new PlayerTaskData()
            {
                taskId = task.TaskId,
                taskState = (TaskState)task.TaskState,
                isRewardClaimed = false,
                progress = new PlayerTaskProgress()
                {
                    progressType = (TaskProgressType)task.ProgressTargets.ProgressType,
                    targetParam = task.ProgressTargets.TargetParam,
                    currentValue = task.ProgressTargets.TargetValue
                }
            };
            //如果玩家任务列表中有相同的任务
            if (_playerTaskDatas.Exists(t => t.taskId == task.TaskId))
            {
                _playerTaskDatas[task.TaskId] = playerTask;
            }
            else
            {
                //添加到玩家任务列表并保存
                _playerTaskDatas.Add(playerTask);
            }
            //在Unity主线程执行
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //通知UI更新
                UIManager.Instance.Find<TaskPanel>().OnTaskLoading(playerTask);
            });

            //任务完成?
            if (playerTask.taskState == TaskState.Completed)
            {
                //在Unity主线程执行
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    //通知UI更新
                    UIManager.Instance.Find<TaskPanel>().OnTaskCompleted(playerTask);
                });
            }
        }
    }
    #endregion

    #region 放弃任务
    /// <summary>
    /// 放弃任务
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    public void AbanbonTaskRequest(int taskId)
    {
        PlayerTaskData playerTask = _playerTaskDatas.FirstOrDefault(p => p.taskId == taskId);
        if (playerTask == null|| playerTask.taskState!=TaskState.InProgress) return;

        _taskConfigDict.TryGetValue(taskId, out TaskConfig task);
        if (!task.Data.canAbandon) return;

        AbandonTaskRequest res = new AbandonTaskRequest() { TaskId = taskId };
        NetClient.Send(res);

        return;
    }
    /// <summary>
    /// 放弃任务响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _AbandonTaskResponse(Connection conn, AbandonTaskResponse msg)
    {
        if (!msg.Success)
        {
            Debug.Log("拒绝任务失败" + msg.ErrorMsg);
            return;
        }

        PlayerTaskData playerTask = _playerTaskDatas.FirstOrDefault(p => p.taskId == msg.TaskId);
        if (playerTask == null) return;

        _taskConfigDict.TryGetValue(msg.TaskId, out TaskConfig task);
        if (!task.Data.canAbandon) return;

        //移除任务
        _playerTaskDatas.Remove(playerTask);

        //Unity中的逻辑必须在主线程中执行
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // 更新UI
            UIManager.Instance.Find<TaskPanel>().OnTaskAbandoned(msg.TaskId);
        });
        return;
    }
    #endregion

    #region 进度更新

    /// <summary>
    /// 击杀野怪处理事件
    /// </summary>
    private void OnKillMonsterHandler(int playerId,KillMonsterEventArgs args)
    {
        TaskProUpdateRequest(TaskProgressType.KillMonster, args.monsterId, args.killCount);
    }
    /// <summary>
    /// 收集物品处理事件
    /// </summary>
    private void OnCollectItemHandler(int playerId,CollectItemEventArgs args)
    {
        TaskProUpdateRequest(TaskProgressType.CollectItem, args.itemId, args.collectCount);
    }
    /// <summary>
    /// NPC对话处理事件
    /// </summary>
    private void OnTalkNPCHandler(int playerId,int Id)
    {

    }
    /// <summary>
    /// 更新任务进度 预测回滚机制
    /// </summary>
    private void TaskProUpdateRequest(TaskProgressType progressType,int targetParam,int addValue)
    {
        //遍历玩家所有正在执行的任务
        foreach (var playerTask in _playerTaskDatas.
            Where(t=>t.taskState==TaskState.InProgress&&t.progress.progressType== progressType
            &&t.progress.targetParam==targetParam))
        {
            //从字典中获取配置任务
            if (!_taskConfigDict.TryGetValue(playerTask.taskId, out TaskConfig Config)) continue;

            //获取通过策略工厂获取策略实例
            var strategy = TaskStrategyFactory.Instance.GetStrategy(progressType);

            if (strategy.IsCompleted(playerTask, Config)) return;

            //客户端本地预计算进度
            playerTask.progress.currentValue = strategy.CalculateProgress(playerTask,Config,(targetParam, addValue));
            //通知UI更新进度
            UIManager.Instance.Find<TaskPanel>().OnTaskProgressUpdated(playerTask);
            if (strategy.IsCompleted(playerTask, Config))
            {
                playerTask.taskState = TaskState.Completed;
                // 通知UI更新任务状态（显示“领取奖励”按钮）
                UIManager.Instance.Find<TaskPanel>().OnTaskCompleted(playerTask);
            }

            Debug.Log($"客户端预测:进度{playerTask.progress.currentValue}/{Config.Data.progress.targetValue}");

            //向服务器发起更新进度请求
            TaskProUpdateRequest res = new TaskProUpdateRequest()
            {
                TaskId= playerTask.taskId,
                CurrentProgressValues=addValue,
                IsCompleted=false
            };
            NetClient.Send(res);
        }
    }
    /// <summary>
    /// 任务进度更新响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _TaskProUpdateResponse(Connection conn, TaskProUpdateResponse msg)
    {
        if (!msg.Success) return;

        //遍历玩家所有正在执行的任务
        foreach (var playerTask in _playerTaskDatas.Where(t => t.taskState == TaskState.InProgress
        && t.taskId == msg.TaskId))
        {
            //从字典中获取配置任务
            if (!_taskConfigDict.TryGetValue(playerTask.taskId, out TaskConfig Config)) continue;

            //客户端的数据和服务器下发数据进行对比纠正
            playerTask.progress.currentValue = msg.CurrentProgressValues;

            //unity的逻辑在主线程中执行
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //通知UI更新进度
                UIManager.Instance.Find<TaskPanel>().OnTaskProgressUpdated(playerTask);
            });

            //任务完成
            if (msg.IsCompleted&& playerTask.taskState != TaskState.Completed)
            {
                playerTask.taskState = TaskState.Completed;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // 通知UI更新任务状态（显示“领取奖励”按钮）
                    UIManager.Instance.Find<TaskPanel>().OnTaskCompleted(playerTask);
                });
                Debug.Log($"任务完成：TaskId = {playerTask.taskId}");
            }
        }
    }
    #endregion

    #region 发放奖励
    /// <summary>
    /// 领取奖励响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _SubmitTaskResponse(Connection conn, SubmitTaskResponse msg)
    {
        if (!msg.Success)
        {
            Debug.Log($"领取失败:{msg.ErrorMsg}");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            (UIManager.Instance.ShowUI<TipType1Panel>(E_UIPanelLayer.Forefront) as TipType1Panel)
                    .Show("领取失败", $"{msg.ErrorMsg}", () =>
                    {
                        UIManager.Instance.HideUI("TipType1Panel");
                    }));
        }
        PlayerTaskData playerTask = _playerTaskDatas.FirstOrDefault(t => t.taskId == msg.TaskId);
        if (playerTask == null)
        {
            Debug.LogError($"领取奖励失败：任务不存在（TaskId = {msg.TaskId}）");
            return ;
        }
        // 发放奖励（对接外部系统）
        switch (msg.Rewards.RewardType)
        {
            case (Proto.RewardType)RewardType.Gold:
                Debug.Log("奖励金币");
                break;

            case (Proto.RewardType)RewardType.Item:
                Debug.Log("物品");
                break;
        }
        Debug.Log($"发放奖励：{msg.Rewards.RewardType}" + $"（数量：{msg.Rewards.Count}）");

        // 更新任务状态
        playerTask.isRewardClaimed = true;
        playerTask.taskState = TaskState.Finished;

        //主线程中执行unity的程序
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log("进入111111");
            //通知UI更新
            UIManager.Instance.Find<TaskPanel>().OnRewardClaimed(playerTask.taskId);
            (UIManager.Instance.ShowUI<TipType1Panel>(E_UIPanelLayer.Forefront) as TipType1Panel)
                    .Show("领取成功", $"获得:{msg.Rewards.RewardType},（数量：{msg.Rewards.Count}）", () =>
                    {
                        UIManager.Instance.HideUI("TipType1Panel");
                    });
        });
    }
    /// <summary>
    /// 领取奖励请求
    /// </summary>
    public void ClaimRewardRequest(int taskId)
    {
        SubmitTaskRequest res = new SubmitTaskRequest() { TaskId=taskId};
        NetClient.Send(res);
    }
    #endregion

    #region 外部接口
    
    /// <summary>
    /// 获取玩家所有任务数据
    /// </summary>
    /// <returns></returns>
    public List<PlayerTaskData> GetAllPlayerTasks()
    {
        return new List<PlayerTaskData>(_playerTaskDatas); //返回副本避免外部修改
    }
    /// <summary>
    /// 返回所有正常进行的任务
    /// </summary>
    /// <returns></returns>
    public List<PlayerTaskData> GetInProgressTasks()
    {
        return _playerTaskDatas.Where(p => p.taskState == TaskState.InProgress
        || p.taskState == TaskState.Completed).ToList();
    }
    #endregion
}
