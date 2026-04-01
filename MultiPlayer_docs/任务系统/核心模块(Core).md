#### 任务管理器（TaskManager）

- 玩家任务信息列表：存储当前玩家的所有任务信息，进入游戏时从服务器请求； 
```csharp
    private List<PlayerTaskData> _playerTaskDatas = new List<PlayerTaskData>();
```
- 任务配置字典：存储所有的任务配置数据，从本地读取Json文件加载；
```csharp
    private Dictionary<int, TaskConfig> _taskConfigDict => TaskConfigManager.Instance._taskConfigDict;
```
---
##### 事件订阅和执行
---
- 事件的订阅和注销
```csharp
    private void SubscribeEvents()
    {
        TaskEventCenter.OnKillMonster += OnKillMonsterHandler;
        TaskEventCenter.OnCollectItem += OnCollectItemHandler;
        TaskEventCenter.OnTalkNPC += OnTalkNPCHandler;
    }
    private void OnDestroy()
    {
        TaskEventCenter.OnKillMonster -= OnKillMonsterHandler;
        TaskEventCenter.OnCollectItem -= OnCollectItemHandler;
        TaskEventCenter.OnTalkNPC -= OnTalkNPCHandler;
    }
```
- 在任务事件中心里面订阅任务完成的响应事件，任务完成响应由外部进行调用；
```csharp
 //模拟完成任务
        if (Input.GetKeyDown(KeyCode.Alpha1))
        TaskEventCenter.TriggerKillMonster(GameApp.character.entityId,new KillMonsterEventArgs(1001, 1));
        if (Input.GetKeyDown(KeyCode.Alpha2))
        TaskEventCenter.TriggerCollectItem(GameApp.character.entityId,new CollectItemEventArgs(1001, 1));
        if (Input.GetKeyDown(KeyCode.Alpha3))
        TaskEventCenter.TriggerCollectItem(GameApp.character.entityId,new CollectItemEventArgs(1002, 1));
```
- 外部执行任务后回调TaskManager中的响应事件
```csharp
    //完成击败敌人响应事件
    private void OnKillMonsterHandler(int playerId,KillMonsterEventArgs args)
    {
        TaskProUpdateRequest(TaskProgressType.KillMonster, args.monsterId, args.killCount);
    }
    //完成收集任务响应事件
    private void OnCollectItemHandler(int playerId,CollectItemEventArgs args)
    {
        TaskProUpdateRequest(TaskProgressType.CollectItem, args.itemId, args.collectCount);
    }
```
- 在响应事件中会执行任务完成更新函数，请求服务器更新任务进度；
---
##### 激活任务
---
- 接取任务请求，先利用客户端的任务配置表和玩家任务列表预测接取任务结果，然后更新UI显示，再请求服务器接取任务；
``` csharp
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
        //发起接取任务请求到服务端...
```
---
- 接取任务响应，服务器会返回任务接取是否成功和任务状态的数据，使用服务器的数据覆盖本地客户端的数据，然后再重新更新任务显示，避免客户端预测错误导致任务接取异常；
```csharp
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
```

##### 获取任务信息
---
- 任务请求：请求服务器返回玩家的任务数据；
```csharp
   public void TaskDataRequest()
    {
        TaskDataRequest res = new TaskDataRequest();
        NetClient.Send(res);
    }
```
---
- 任务信息响应：使用服务器返回的任务初始化玩家任务信息列表，更新任务面板显示；
```csharp
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
```
##### 放弃任务
---
- 任务放弃请求：客户端先判断该任务是否存在或者是否可放弃，然后再请求服务器放弃任务；
```csharp
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
```
---
- 任务放弃响应：客户端收到任务放弃响应后，先从玩家任务列表中移除该任务，然后更新任务面板显示；

```csharp
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
```

##### 任务进度更新
---
- 任务进度更新请求：遍历玩家任务列表找到目标任务，对比任务进度是否超出任务配置表中进度，客户端先预测任务更新结果，然后再请求服务器更新任务进度；
```csharp
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
```
---
- 任务进度更新响应：收到服务端下发的任务进度后，和本地预测结果进行对比，如果正确的话则返回出去，否则就使用服务端的数据覆盖本地数据，重新更新任务面板的进度；
```csharp
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
```
##### 奖励领取
---
- 奖励领取请求：向服务端发送任务Id，请求任务奖励领取，奖励领取需要服务端绝对权威，避免影响玩家体验；
```csharp
    public void ClaimRewardRequest(int taskId)
    {
        SubmitTaskRequest res = new SubmitTaskRequest() { TaskId=taskId};
        NetClient.Send(res);
    }
```
---
- 奖励领取响应：如果任务奖励领取失败则出现领取失败弹窗，如果领取成功则更新任务进度，再显示领取成功弹窗；
```csharp
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
```