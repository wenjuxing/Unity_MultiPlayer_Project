using Assets.Scripts.U3d_scripts;
using Proto;
using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对话总控制器 层级通信中转
/// </summary>
public class DialogueSystem : SingletonBase<DialogueSystem>
{
    //核心模块
    private IDialogueFactory _dialogueFactory;
    public DialogueManager _dialogueManager;
    public DialoguePanel dialogueUI;
    private IEventBus _eventBus;

    private void Awake()
    {
        UIManager.Instance.ShowUI<DialoguePanel>();
        UIManager.Instance.HideUI("DialoguePanel");
        dialogueUI = UIManager.Instance.Find<DialoguePanel>();
        //初始化基础设施
        _dialogueFactory = new DialogueFactory();
        _eventBus = new EventBus();
        //初始化管理
        _dialogueManager = new DialogueManager(_dialogueFactory, _eventBus);

        //对话网络传输信息
        MessageRouter.Instance.Subscribe<DialogueResponse>(_DialogueResponse);
        MessageRouter.Instance.Subscribe<DialogueUpdateResponse>(_DialogueUpdateResponse);

        //注册订阅事件
        _eventBus.Subscribe<DialogueShowOptionsEvent>(OnDialogueShowOptions);
        _eventBus.Subscribe<DialogueShowEvent>(OnDialogueShow);
        _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        dialogueUI.OnNextButtonClick += () => { _eventBus.Publish(new DialogueNextClickEvent()); };
    }
    private void OnDestroy()
    {
        // 取消事件注册，防止内存泄漏
        _dialogueManager?.UnregisterEvents();
        _eventBus.Unsubscribe<DialogueShowEvent>(OnDialogueShow);
        _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
        _eventBus.Unsubscribe<DialogueShowOptionsEvent>(OnDialogueShowOptions);
    }
    #region 回调事件
    /// <summary>
    /// 显示选项
    /// </summary>
    /// <param name="obj"></param>
    public void OnDialogueShowOptions(DialogueShowOptionsEvent obj)
    {
        UIManager.Instance.Find<DialoguePanel>()
            .ShowOptions(obj.options,obj.onSelect);
    } 
    /// <summary>
    /// 显示对话
    /// </summary>
    /// <param name="obj"></param>
    public void OnDialogueShow(DialogueShowEvent obj)
    {
        UIManager.Instance.Find<DialoguePanel>()
            .ShowDialogue(obj.characterName, obj.content, obj.characterId);
    }
    /// <summary>
    /// 隐藏对话面板
    /// </summary>
    /// <param name="obj"></param>
    public void OnDialogueEnded(DialogueEndedEvent obj)
    {
        UIManager.Instance.Find<DialoguePanel>().HIdeDialoguePanel();
    }
    #endregion

    #region 外部调用
    /// <summary>
    /// 开始章节对话
    /// </summary>
    public void StartDialogue(int chapterId)
    {
        int groupId = _dialogueFactory.GetChapter(chapterId).firstGroupId;
        _eventBus.Publish(new DialogueGroupLoadRequestEvent 
        {
            groupId=groupId,
            onOptionsSelected= OnGroupDialogueFinished
        });
        UIManager.Instance.ShowUI<DialoguePanel>();
    }
    /// <summary>
    /// 开启网络章节对话请求
    /// </summary>
    public void StartNetDialogueRequest(int chapterId)
    {
        //发起对话请求
        DialogueRequest res = new DialogueRequest();
        res.Id = GameApp.playerId;
        res.ChapterId = chapterId;
        res.GroupId = _dialogueFactory.GetChapter(chapterId).firstGroupId;
        NetClient.Send(res);
    }
    /// <summary>
    /// 对话响应信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _DialogueResponse(Connection conn, DialogueResponse msg)
    {
        Debug.Log($"对话响应结果{msg.IsCompleted}");
        if (msg.IsCompleted)
        {
            Debug.Log(msg.ErrorMsg);
            return;
        }
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //开启对话
            StartDialogue(msg.ChapterId);
        });
    }
    /// <summary>
    /// 对话进度更新响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _DialogueUpdateResponse(Connection conn, DialogueUpdateResponse msg)
    {
        if (msg.Success)
            Debug.Log("对话进度更新成功!");
        else
            Debug.LogError(msg.ErrorMsg);
    }
    /// <summary>
    /// 完成本组对话 跳转至下一组
    /// </summary>
    private void OnGroupDialogueFinished(DialogueOption option)
    {
        _dialogueManager.onOptionSelected(option);
    }
    #endregion
}
