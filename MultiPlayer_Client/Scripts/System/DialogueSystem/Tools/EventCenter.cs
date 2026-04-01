using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

/// <summary>
/// 事件中心 - 线程安全的事件管理系统，确保所有事件在主线程执行
/// </summary>
public class EventCenter : SingletonBase<EventCenter>
{
    // 存储事件类型与对应监听器集合（使用线程安全的HashSet）
    private readonly Dictionary<Type, HashSet<Delegate>> _eventDictionary = new Dictionary<Type, HashSet<Delegate>>();
    // 主线程标识（用于验证事件执行线程）
    private int _mainThreadId;
    // 事件队列（用于非主线程发布事件时暂存）
    private readonly Queue<Action> _eventQueue = new Queue<Action>();
    // 队列操作锁
    private readonly object _queueLock = new object();
    // 初始化状态标识
    private bool _isInitialized = false;

    private  void Awake()
    {
        // 记录主线程ID
        _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        // 启动事件处理协程
        StartCoroutine(ProcessEventQueue());
    }

    /// <summary>
    /// 初始化事件中心，注册核心事件
    /// </summary>
    public void Init()
    {
        if (_isInitialized) return;

        Cleanup();
        RegisterCoreEvents();

        _isInitialized = true;
        Debug.Log("EventCenter initialized successfully");
    }

    /// <summary>
    /// 注册对话系统核心事件
    /// </summary>
    private void RegisterCoreEvents()
    {
        if (DialogueSystem.Instance == null)
        {
            Debug.LogError("DialogueSystem instance is null, cannot register core events");
            return;
        }

        var dialogueSystem = DialogueSystem.Instance;
        var dialogueManager = dialogueSystem._dialogueManager;

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance is null, cannot register core events");
            return;
        }

        // 注册核心事件监听器
        Subscribe<DialogueShowEvent>(dialogueSystem.OnDialogueShow);
        Subscribe<DialogueNextClickEvent>(dialogueManager.OnNextClick);
        Subscribe<DialogueGroupLoadRequestEvent>(dialogueManager.OnGroupLoadRequest);
        Subscribe<DialogueEndedEvent>(dialogueSystem.OnDialogueEnded);
        Subscribe<DialogueShowOptionsEvent>(dialogueSystem.OnDialogueShowOptions);
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="listener">事件监听器</param>
    public void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        if (listener == null)
        {
            Debug.LogError("Event listener cannot be null");
            return;
        }

        var eventType = typeof(T);
        lock (_eventDictionary)
        {
            if (!_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType] = new HashSet<Delegate>();
            }

            if (!_eventDictionary[eventType].Contains(listener))
            {
                _eventDictionary[eventType].Add(listener);
                Debug.Log($"Subscribed to event: {eventType.Name}");
            }
            else
            {
                Debug.LogWarning($"Duplicate subscription to event: {eventType.Name}");
            }
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="listener">事件监听器</param>
    public void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        if (listener == null)
        {
            Debug.LogError("Event listener cannot be null");
            return;
        }

        var eventType = typeof(T);
        lock (_eventDictionary)
        {
            if (_eventDictionary.TryGetValue(eventType, out var listeners))
            {
                listeners.Remove(listener);
                Debug.Log($"Unsubscribed from event: {eventType.Name}");

                if (listeners.Count == 0)
                {
                    _eventDictionary.Remove(eventType);
                }
            }
        }
    }

    /// <summary>
    /// 发布事件（自动确保在主线程执行）
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    public void Publish<T>(T eventData) where T : IEvent
    {
        if (eventData == null)
        {
            Debug.LogError("Event data cannot be null");
            return;
        }

        // 如果当前在主线程，直接执行；否则加入队列
        if (System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            ExecuteEvent(eventData);
        }
        else
        {
            lock (_queueLock)
            {
                _eventQueue.Enqueue(() => ExecuteEvent(eventData));
            }
        }
    }

    /// <summary>
    /// 执行事件（仅在主线程调用）
    /// </summary>
    private void ExecuteEvent<T>(T eventData) where T : IEvent
    {
        var eventType = typeof(T);
        List<Delegate> listenersCopy;

        lock (_eventDictionary)
        {
            if (!_eventDictionary.TryGetValue(eventType, out var listeners))
            {
                Debug.LogWarning($"No listeners for event: {eventType.Name}");
                return;
            }

            // 创建监听器副本，避免执行过程中集合被修改
            listenersCopy = listeners.ToList();
        }

        Debug.Log($"Executing event: {eventType.Name} with {listenersCopy.Count} listeners");

        foreach (var listener in listenersCopy)
        {
            try
            {
                var typedListener = listener as Action<T>;
                typedListener?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing event {eventType.Name}: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// 处理事件队列的协程（每帧执行一次）
    /// </summary>
    private IEnumerator ProcessEventQueue()
    {
        while (true)
        {
            // 每帧处理队列中的所有事件
            while (true)
            {
                Action eventAction = null;
                lock (_queueLock)
                {
                    if (_eventQueue.Count > 0)
                    {
                        eventAction = _eventQueue.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }

                if (eventAction != null)
                {
                    try
                    {
                        eventAction.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing event queue: {ex.Message}");
                    }
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// 清理所有事件订阅
    /// </summary>
    public void Cleanup()
    {
        lock (_eventDictionary)
        {
            _eventDictionary.Clear();
        }

        lock (_queueLock)
        {
            _eventQueue.Clear();
        }

        _isInitialized = false;
        Debug.Log("EventCenter cleaned up");
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}