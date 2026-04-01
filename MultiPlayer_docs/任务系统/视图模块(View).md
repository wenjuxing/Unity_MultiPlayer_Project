
- 任务面板：继承自面板基类，由UIManager创建调用，任务面板由ScrollView+任务ItemUI组成，通过垂直布局组实现任务ItemUI自动排版；
- 任务列表：存储任务面板中每一条任务的数据；
```csharp
 private Dictionary<int, TaskItem> _taskItemDict = new Dictionary<int, TaskItem>();
```
---
##### 创建任务Item
---
- 在协程中通过对象池获取任务Item预制体，然后给任务Item预制体添加任务映射脚本，初始化任务Item数据；
```csharp
IEnumerator CreateTaskItem(PlayerTaskData playerTask,TaskConfig config)
    {
        var res = Resources.LoadAsync<GameObject>("Prefabs/Task/TaskItem");
        yield return res;
        var item = ObjectPoolsManager.Instance.Spawn(res.asset as GameObject,Vector3.zero,Quaternion.identity, Parent);
        TaskItem taskItem = item.GetComponent<TaskItem>();
        taskItem.Init(playerTask, config);
        _taskItemDict.Add(playerTask.taskId,taskItem);
    }
```

##### 任务详情显示
---
通过TaskId获取任务详情，然后通过TextPro显示，使用TextPro的自适用文字大小，根据内容动态调整字体大小；

```csharp
public void OnShowTaskDetail(PlayerTaskData playerTask)
    {
        TaskConfig config = TaskConfigManager.Instance.GetTaskConfig(playerTask.taskId);
        if (config == null) return;

        //更新文本显示
        TaskDescription.text = config.Data.taskDesc;
    }
```