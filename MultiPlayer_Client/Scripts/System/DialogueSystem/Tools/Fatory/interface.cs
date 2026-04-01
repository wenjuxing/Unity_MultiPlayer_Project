using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 对话UI接口 表现层抽象
/// </summary>
public interface IDialogueView
{
    void ShowDialogue(string characterName, string content); //显示单条对话
    void ShowOptions(List<DialogueOption> options,Action<DialogueOption> onSelect); //显示选项
    void HIdeDialogue(); //隐藏对话UI
    bool IsVisible { get; }
}

/// <summary>
/// 事件总线接口 应用层抽象
/// </summary>
public interface IEventBus
{
    void Subscribe<T>(Action<T> listener) where T : IEvent;
    void Unsubscribe<T>(Action<T> listener) where T : IEvent;
    void Publish<T>(T evt) where T : IEvent;
}

/// <summary>
/// 对话工厂接口 数据层抽象
/// </summary>
public interface IDialogueFactory
{
    DialogueMain GetChapter(int chapterId);
    DialogueGroup GetGroup(int groupId);
    List<DialogueData> GetGroupDatas(int groupId);
}
/// <summary>
/// 对话头像加载接口
/// </summary>
public interface IAvatarService
{
    Task<Sprite> GetAvatarByIdAsync(int speakerId);
}
/// <summary>
/// 事件基类
/// </summary>
public interface IEvent { };