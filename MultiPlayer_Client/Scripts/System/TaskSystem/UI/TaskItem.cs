using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public delegate void OnClaimReward(int taskId);
public delegate void OnAbandonTask(int taskId);
public delegate void OnAcceptTask(int taskId);
public delegate void OnClick(PlayerTaskData data);
public class TaskItem : MonoBehaviour
{
    [Header("组件")]
    public TextMeshProUGUI Title;
    public TextMeshProUGUI ProgressText;
    public Text rewardCount;
    public Button AbandonBtn;
    public Button ClaimBtn;
    public Button AcceptBtn;
    public Image StateBg;
    public Image rewardIcon;

    private PlayerTaskData _taskData;
    private TaskConfig _taskConfig;

    public static event OnClaimReward OnClaimReward;
    public static event OnAbandonTask OnAbandonTask;
    public static event OnAcceptTask OnAcceptTask;
    public static event OnClick OnClick;
    private void Awake()
    {
        StateBg.gameObject.SetActive(false);

        //绑定按钮事件
        ClaimBtn.onClick.AddListener(()=> { OnClaimReward?.Invoke(_taskData.taskId); });
        AbandonBtn.onClick.AddListener(() => { OnAbandonTask?.Invoke(_taskData.taskId); });
        AcceptBtn.onClick.AddListener(() => { OnAcceptTask?.Invoke(_taskData.taskId); });
        GetComponent<Button>().onClick.AddListener(() => { OnClick?.Invoke(_taskData); });
    }
    /// <summary>
    /// 初始化任务条
    /// </summary>
    public void Init(PlayerTaskData playerTask,TaskConfig taskConfig)
    {
        this._taskData = playerTask;
        this._taskConfig = taskConfig;
        this.Title.text = taskConfig.Data.taskName;
        this.rewardCount.text = taskConfig.Data.rewards.count.ToString();
        this.rewardIcon.sprite = Resources.Load<Sprite>(DataManager.Instance.Items[taskConfig.Data.taskId].Icon);

        UpdateProgress(playerTask);
        UpdateState(playerTask.taskState);
    }
    /// <summary>
    /// 更新状态
    /// </summary>
    /// <param name="taskState"></param>
    public void UpdateState(TaskState taskState)
    {
        switch (taskState)
        {
            case TaskState.InActive:
                AcceptBtn.gameObject.SetActive(true);
                ClaimBtn.gameObject.SetActive(false);
                AbandonBtn.gameObject.SetActive(_taskConfig.Data.canAbandon);
                break;
            case TaskState.InProgress:
                AcceptBtn.gameObject.SetActive(false);
                ClaimBtn.gameObject.SetActive(false);
                AbandonBtn.gameObject.SetActive(_taskConfig.Data.canAbandon);
                break;
            case TaskState.Completed:
                ClaimBtn.gameObject.SetActive(true);
                AbandonBtn.gameObject.SetActive(false);
                break;
            case TaskState.Finished:
                AcceptBtn.gameObject.SetActive(false);
                ClaimBtn.gameObject.SetActive(false);
                AbandonBtn.gameObject.SetActive(false);
                StateBg.gameObject.SetActive(true);
                ProgressText.gameObject.SetActive(false);
                break;
        }
    }
    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="playerTask"></param>
    public void UpdateProgress(PlayerTaskData playerTask)
    {
        var config = _taskConfig.Data.progress;
        var progress = playerTask.progress;
        string progressDesc = $"{config.progressDesc}({progress.currentValue}/{config.targetValue})";
        ProgressText.text = progressDesc;
    }
}
