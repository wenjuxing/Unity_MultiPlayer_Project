using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件总线实现 （Unity主线程安全）
/// </summary>
public class EventBus : IEventBus
{
    private readonly Dictionary<Type, Delegate> _eventDictionary = new Dictionary<Type, Delegate>();
    public void Publish<T>(T evt) where T : IEvent
    {
        //确保在Unity主线程中执行
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var type = typeof(T);
            if (_eventDictionary.TryGetValue(type,out Delegate del))
            {
                (del as Action<T>)?.Invoke(evt);
            }
        });
    }

    public void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        var type = typeof(T);
        if (!_eventDictionary.ContainsKey(type))
        {
            _eventDictionary[type] = null;
            _eventDictionary[type] = (Action<T>)_eventDictionary[type] + listener;
        }
    }

    public void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        var type = typeof(T);
        if (!_eventDictionary.ContainsKey(type)) return;
        _eventDictionary[type] = (Action<T>)_eventDictionary[type] - listener;
    }
}

/// <summary>
/// 开始对话事件 （传递章节Id）
/// </summary>
public class DialogueStartEvent:IEvent
{
    public int chapterId { get; set; }
}
/// <summary>
/// 对话组加载完成事件(传递当前对话组和选项回调事件)
/// </summary>
public class DialogueGroupLoadRequestEvent :IEvent
{
    public int groupId { get; set; }
    public Action<DialogueOption> onOptionsSelected { get; set; }
}
/// <summary>
/// 章节结束事件 （传递章节Id）
/// </summary>
public class DialogueEndedEvent:IEvent
{
    public int chapterId { get; set; }
}
/// <summary>
/// 显示单条对话事件
/// </summary>
public class DialogueShowEvent:IEvent
{
    public string characterName { get; set; }
    public string content { get; set; }
    public int characterId { get; set; }
}
/// <summary>
/// 用户点击下一步事件
/// </summary>
public class DialogueNextClickEvent:IEvent { }
/// <summary>
/// 显示对话选项事件
/// </summary>
public class DialogueShowOptionsEvent : IEvent
{
    public List<DialogueOption> options;
    public Action<DialogueOption> onSelect;
}