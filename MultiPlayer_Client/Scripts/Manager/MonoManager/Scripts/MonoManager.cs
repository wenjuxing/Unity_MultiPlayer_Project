using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Mono管理器
/// </summary>
public class MonoManager : SingletonBase<MonoManager>
{
    //声明委托事件
    event UnityAction updateEvent;
    event UnityAction fixUpdateEvent;
    event UnityAction lateUpdateEvent;
    //构造方法私有化，防止外部new对象
    private MonoManager() { }
    private void Update()
    {
        updateEvent?.Invoke();
    }
    private void FixedUpdate()
    {
        fixUpdateEvent?.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }
    /// <summary>
    /// 让外部类通过它开启协成
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public Coroutine Start_Coroutine(IEnumerator routine)
    {
      return  StartCoroutine(routine);
    }
    /// <summary>
    /// 让外部类通过它停止协程
    /// </summary>
    /// <param name="routine"></param>
    public void Stop_Coroutine(Coroutine routine)
    {
        StopCoroutine(routine);
    }
    /// <summary>
    /// 关闭所有协程
    /// </summary>
    public void StopAll_Coroutine()
    {
        StopAllCoroutines();
    }
    /// <summary>
    /// 让外部类通过它开启Update函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void AddUpdateListener(UnityAction action)
    {
        updateEvent += action;
    }
    /// <summary>
    /// 让外部通过它关闭Update函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void RemoveUpdate(UnityAction action)
    {
        updateEvent -= action;
    }
    /// <summary>
    /// 让外部通过它关闭所有Update函数
    /// </summary>
    public void RemoveAllUpdate()
    {
        Debug.Log("关闭所有Update函数！");
        updateEvent = null;
    }
    /// <summary>
    /// 让外部类通过它开启FixUpdate函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void AddFixUpdateListener(UnityAction action)
    {
        fixUpdateEvent += action;
    }
    /// <summary>
    /// 让外部通过它关闭FixUpdate函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void RemoveFixUpdate(UnityAction action)
    {
        fixUpdateEvent -= action;
    }
    /// <summary>
    /// 让外部通过它关闭所有FixUpdate函数
    /// </summary>
    public void RemoveAllFixUpdate()
    {
        Debug.Log("关闭所有fixUpdate函数！");
        fixUpdateEvent = null;
    }
    /// <summary>
    /// 让外部类通过它开启LateUpdate函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void AddLateUpdateListener(UnityAction action)
    {
        lateUpdateEvent += action;
    }
    /// <summary>
    /// 让外部通过它关闭LateUpdate函数
    /// </summary>
    /// <param name="unityAction"></param>
    public void RemoveLateUpdate(UnityAction action)
    {
        lateUpdateEvent -= action;
    }
    /// <summary>
    /// 让外部通过它关闭所有LateUpdate函数
    /// </summary>
    public void RemoveAllLateUpdate()
    {
        Debug.Log("关闭所有LateUpdate函数！");
        lateUpdateEvent = null;
    }
}

