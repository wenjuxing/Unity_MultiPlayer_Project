using Assets.Scripts.U3d_scripts;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对话管理器
/// </summary>
public class DialogueManager
{
    private readonly IDialogueFactory _dialogueFactory; //对话工厂
    private readonly IEventBus _eventBus;
    private DialogueGroup _currentGroup;
    private int _currentChapterId; //章节Id
    //当前组内所有的对话数据
    private List<DialogueData> _currentGroupDatas;
    //对话完成回调
    private Action<DialogueOption> _onGroupDialogueFinished;
    // 当前组内展示到第几句
    private int _currentDialogueIndex; 

    /// <summary>
    /// 依赖抽象
    /// </summary>
    /// <param name="dialogueFactory"></param>
    /// <param name="eventBus"></param>
    public DialogueManager(IDialogueFactory dialogueFactory,IEventBus eventBus)
    {
        this._dialogueFactory = dialogueFactory;
        this._eventBus = eventBus;
        RegisterEvent();
    }

    #region 事件注册
    private void RegisterEvent()
    {
        _eventBus.Subscribe<DialogueGroupLoadRequestEvent>(OnGroupLoadRequest);
        _eventBus.Subscribe<DialogueNextClickEvent>(OnNextClick);
    }
    /// <summary>
    /// 取消注册（防止内存泄漏）
    /// </summary>
    public void UnregisterEvents()
    {
        _eventBus.Unsubscribe<DialogueGroupLoadRequestEvent>(OnGroupLoadRequest);
        _eventBus.Unsubscribe<DialogueNextClickEvent>(OnNextClick);
    }
    /// <summary>
    /// 用户点击下一步
    /// </summary>
    /// <param name="obj"></param>
    public void OnNextClick(DialogueNextClickEvent obj)
    {
        Debug.Log("当前对话索引" + _currentDialogueIndex);
        //先检查对话列表是否为空
        if (_currentGroupDatas == null || _currentGroupDatas.Count == 0)
        {
            Debug.Log("对话列表为空，无法执行下一句对话");
            EndDialogue();
            return;
        }

        //检查索引是否到最后一条
        if (_currentDialogueIndex >= _currentGroupDatas.Count - 1)
        {
            Debug.Log("已到对话最后一句，结束对话");
            EndDialogue();
            _currentDialogueIndex = 0;
            return;
        }

        //索引自增并检查边界
        _currentDialogueIndex++;
        if (_currentDialogueIndex < 0 || _currentDialogueIndex >= _currentGroupDatas.Count)
        {
            Debug.LogError($"对话索引越界！当前索引: {_currentDialogueIndex}，列表长度: {_currentGroupDatas.Count}");
            _currentDialogueIndex = Mathf.Clamp(_currentDialogueIndex, 0, _currentGroupDatas.Count - 1); // 修正索引
            return;
        }

        ShowCurrentDialogue();
    }
    /// <summary>
    /// 跳转下一组
    /// </summary>
    /// <param name="obj"></param>
    public void OnGroupLoadRequest(DialogueGroupLoadRequestEvent obj)
    {
        _onGroupDialogueFinished = obj.onOptionsSelected;
        //加载下一组
        LoadGroup(obj.groupId);
    }
    #endregion

    /// <summary>
    /// 显示当前对话
    /// </summary>
    private void ShowCurrentDialogue()
    {
        //当前对话组完成
        if (_currentDialogueIndex == _currentGroupDatas.Count-1)
        {
            Debug.Log("数量 "+ _currentGroupDatas.Count);
            int currentGroupId = _currentGroup.id;
            if (DialogueDataModel.dialogueOptionMap.TryGetValue(currentGroupId, out var options) && options.Count > 0)
            {
                var currentData1 = _currentGroupDatas[_currentDialogueIndex];
                _eventBus.Publish(new DialogueShowEvent
                {
                    characterName = currentData1.characterName,
                    content = currentData1.content,
                    characterId = currentData1.characterId
                });

                _eventBus.Publish(new DialogueShowOptionsEvent
                {
                    options = options,
                    onSelect = _onGroupDialogueFinished
                });
                return;
            }
            LoadGroup(_currentGroup.nextGroupId);
            return;
        }

        //继续对话
        var currentData = _currentGroupDatas[_currentDialogueIndex];
        _eventBus.Publish(new DialogueShowEvent
        {
            characterName = currentData.characterName,
            content = currentData.content,           
            characterId = currentData.characterId
        });

        //保存当前对话组的对话信息
        if (_dialogueFactory.GetChapter(_currentChapterId).needSyncServer)
        {
            DialogueUpdateRequest res = new DialogueUpdateRequest();
            res.Id = GameApp.playerId;
            res.ChapterId = _currentChapterId;
            res.GroupId = _currentGroup.id;
            NetClient.Send(res);
        }
    }
    /// <summary>
    /// 加载对话组
    /// </summary>
    private void LoadGroup(int groupId)
    {
        //初始化索引显示第一句
        _currentDialogueIndex = 0;
        _currentGroup = _dialogueFactory.GetGroup(groupId);
        _currentChapterId = _currentGroup.chapterId;
       
        //对话组为空则对话结束
        if (_currentGroup == null)
        {
            Debug.Log("LoadGroup：_currentGroup为空");
            EndDialogue();
            return;
        }
        Debug.Log($"_currentGroup.id：{_currentGroup.id}");

        //获取该组的所有对话数据
        _currentGroupDatas = _dialogueFactory.GetGroupDatas(groupId);
        if (_currentGroupDatas==null||_currentGroupDatas.Count==0)
        {
            Debug.Log($"DialogueManager：组ID {groupId} 无对话数据，直接跳组");
            _onGroupDialogueFinished?.Invoke(null);
            return;
        }
        ShowCurrentDialogue();
    }
    /// <summary>
    /// 处理选项选择
    /// </summary>
    public void onOptionSelected(DialogueOption option)
    {
        //索引自增并检查边界
        ++_currentDialogueIndex;
       
        //非空执行事件（获取物品 开启任务）
        if (option != null)
        {
            ExecuteTriggerEvent(option.triggerEvent);
        }

        // 计算目标组ID
        //优先使用选项数据中的目标Id
        //若选项配置错误则使用当前对话组的下一组对话Id
        int targetId;
        if (option == null)
        {
            targetId = -1;
        }
        else
        {
            targetId = option.targetGroupId;
        }

        Debug.Log($"选项跳转目标组ID: {targetId}");
        //三层校验
        //首次校验
        if (targetId==-1)
        {
            EndDialogue();
            return;
        }
        //二次校验
        DialogueGroup group= DataManager.Instance.dialogueGroups[targetId];
        if (group==null)
        {
            Debug.LogError($"跳错拦截：目标对话组ID={targetId} 不存在！请检查配置");
            EndDialogue(); 
            return;
        }
        //三次效验
        List<DialogueData> dialogues=_dialogueFactory.GetGroupDatas(group.id);
        if (dialogues==null||string.IsNullOrEmpty(group.nextGroupId.ToString())||dialogues.Count==0)
        {
            Debug.LogError($"跳错拦截：对话组ID={targetId} 配置不完整（无台词/无ID）");
            EndDialogue();
            return;
        }
        LoadGroup(targetId);
    }
    /// <summary>
    /// 执行触发事件
    /// </summary>
    /// <param name="eventStr"></param>
    private void ExecuteTriggerEvent(string eventStr)
    {
        if (string.IsNullOrEmpty(eventStr)) return;
        //通过事件工厂创建事件
        IEventTrigger trigger = EventTriggerFactory.Create(eventStr);
        trigger?.Execute();
    }
    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        //发布对话结束事件
        _eventBus.Publish(new DialogueEndedEvent { chapterId = _currentChapterId });
        //重置状态
        _currentGroup = null;
        _currentDialogueIndex = 0;
        _currentGroupDatas = null;
        _onGroupDialogueFinished = null;
    }
}
