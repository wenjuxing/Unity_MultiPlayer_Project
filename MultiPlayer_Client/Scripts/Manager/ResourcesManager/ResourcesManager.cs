using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 资源管理器
/// </summary>
public class ResourcesManager : SingletonBase<ResourcesManager>
{
    #region 同步加载单个资源
    /// <summary>
    /// 同步加载文件夹中的资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>s
    public Object Load(string path)
    {
        return Resources.Load(path);
    }
    /// <summary>
    /// 同步加载文件夹指定资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="typeInstance"></param>
    /// <returns></returns>
    public Object Load(string path,System.Type typeInstance)
    {
        return Resources.Load(path, typeInstance);
    }
    public T Load<T>(string path)where T:Object
    {
        return Resources.Load<T>(path);
    }
    /// <summary>
    /// 加载指定路径下文件中的所有资源包括子孙的资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Object[] LoadAll(string path)
    {
        return Resources.LoadAll(path);
    }
    /// <summary>
    /// 加载指定路径下文件夹中所有的指定资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="typeInstance"></param>
    /// <returns></returns>
    public Object[] LoadAll(string path,System.Type typeInstance)
    {
        return Resources.LoadAll(path, typeInstance);
    }
    #endregion
    #region 异步加载资源
    /// <summary>
    /// 异步加载单个资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callBack"></param>
    public void LoadAsync(string path,UnityAction<Object> callBack=null )
    {
        MonoManager.Instance.Start_Coroutine(LoadAsyncCoroutine(path,callBack));
    }
    private IEnumerator LoadAsyncCoroutine(string path, UnityAction<Object> callBack=null)
    {
        //resourceRequest是unity异步加载时返回的，用来跟踪资源加载的进程
        ResourceRequest resourceRequest = Resources.LoadAsync(path);
        yield return resourceRequest;
        callBack?.Invoke(resourceRequest.asset);
    }
    /// <summary>
    /// 异步加载指定类型资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callBack"></param>
    public void LoadAsync<T>(string path,UnityAction<T> callBack)where T:Object
    {
        MonoManager.Instance.Start_Coroutine(LoadAsyncCoroutine(path, callBack));
    }
    private IEnumerator LoadAsyncCoroutine<T>(string path, UnityAction<T> callBack)where T:Object
    {
       ResourceRequest resourceRequest=Resources.LoadAsync<T>(path);
        yield return resourceRequest;
        callBack?.Invoke(resourceRequest.asset as T);
    }
    /// <summary>
    /// 异步加载指定类型资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="callBack"></param>
    public void LoadAsync<T>(string path, UnityAction<T,Vector3> callBack,Vector3 position) where T : Object
    {
        MonoManager.Instance.Start_Coroutine(LoadAsyncCoroutine(path, callBack,position));
    }
    private IEnumerator LoadAsyncCoroutine<T>(string path, UnityAction<T, Vector3> callBack,Vector3 position) where T : Object
    {
        ResourceRequest resourceRequest = Resources.LoadAsync<T>(path);
        yield return resourceRequest;
        callBack?.Invoke(resourceRequest.asset as T,position);
    }
    #endregion
    #region 异步卸载资源
    public void UnLoadUnusedAssets(UnityAction callBack)
    {
        MonoManager.Instance.Start_Coroutine(UnLoadUnusedAssetsCoroutine(callBack));
    }
    private IEnumerator UnLoadUnusedAssetsCoroutine(UnityAction callBack=null)
    {
        //异步卸载不使用的资源
        AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
        //如果没有卸载完成，就每帧检查一次，检查完就把控制权交回主线程，检查时间极短
        while (asyncOperation.progress<1)
        {
            yield return null;
        }
        //执行回调事件
        callBack.Invoke();
    }
    #endregion
}
