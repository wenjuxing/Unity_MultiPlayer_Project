using UnityEngine;
/// <summary>
/// 启动任务类型
/// </summary>
public class StartTaskTrigger: IEventTrigger
{
    //任务Id
    private readonly string _taskId;

    public StartTaskTrigger(string taskId)
    {
        this._taskId = taskId;
    }

    public void Execute()
    {
        //通知任务系统执行任务
        Debug.Log("执行下一个任务");
    }
}