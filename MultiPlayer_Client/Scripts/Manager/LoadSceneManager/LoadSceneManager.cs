using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景管理器
/// </summary>
public class LoadSceneManager : SingletonBase<LoadSceneManager>
{
    #region 同步加载场景
    /// <summary>
    /// 重新加载当前场景
    /// </summary>
    public void LoadActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    /// <summary>
    /// 加载下一个场景
    /// </summary>
    /// <param name="Cyclical"></param>
   public void LoadNextScene(bool Cyclical=false )
    {
        //获取下一个场景的索引
        int SceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneIndex>SceneManager.sceneCountInBuildSettings-1)
        {
            if (Cyclical)
            {
                //如果超出当前索引则归0
                SceneIndex = 0;
            }
            else
            {
                Debug.LogError("超出当前索引");
                return;
            }
        }
        SceneManager.LoadScene(SceneIndex);
    }
    /// <summary>
    /// 加载上一个场景
    /// </summary>
    /// <param name="Cyclical"></param>
    public void LoadPreviewScene(bool Cyclical = false)
    {
        int SceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (SceneIndex<0)
        {
            if (Cyclical)
                SceneIndex = SceneManager.sceneCountInBuildSettings - 1;
            else
            {
                Debug.LogError("超出索引范围");
                return;
            } 
        }
        SceneManager.LoadScene(SceneIndex);
    }
    #endregion
    #region 异步加载场景
    /// <summary>
    /// 异步加载场景(索引)
    /// </summary>
    /// <param name="BuildIndex"></param>
    /// <param name="loading"></param>
    /// <param name="complete"></param>
    /// <param name="setActiveAfterCompleted"></param>
    /// <param name="Mode"></param>
    public void LoadAsyncScene(int BuildIndex,UnityAction<float> loading=null,
    UnityAction<AsyncOperation> complete=null,bool setActiveAfterCompleted=true, LoadSceneMode Mode= LoadSceneMode.Single)
    {
        if (!isSceneBuildIndexValid(BuildIndex)) return;
        
        MonoManager.Instance.Start_Coroutine(LoadAsyncSceneCoroutine(BuildIndex,loading,complete,setActiveAfterCompleted,Mode));
    }
    private IEnumerator LoadAsyncSceneCoroutine(int BuildIndex, UnityAction<float> loading = null,
    UnityAction<AsyncOperation> complete = null, bool setActiveAfterCompleted = true, LoadSceneMode Mode = LoadSceneMode.Single)
    {
        //获取异步加载返回的一个异步操作
      AsyncOperation asyncOperation= SceneManager.LoadSceneAsync(BuildIndex,Mode);
        //先把场景设为false
        asyncOperation.allowSceneActivation = false;
        while (asyncOperation.progress<0.9f)
        {
            loading?.Invoke(asyncOperation.progress);
            //等到本帧末尾再回来检查
            yield return new WaitForEndOfFrame();
        }
        loading?.Invoke(1f);
        asyncOperation.allowSceneActivation = setActiveAfterCompleted;
        complete?.Invoke(asyncOperation);
    }
    /// <summary>
    /// 异步加载场景(通过名称)
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loading"></param>
    /// <param name="complete"></param>
    /// <param name="setActiveAfterCompleted"></param>
    /// <param name="Mode"></param>
    public void LoadAsyncScene(string sceneName, UnityAction<float> loading = null,
    UnityAction<AsyncOperation> complete = null, bool setActiveAfterCompleted = true, LoadSceneMode Mode = LoadSceneMode.Single)
    {
        MonoManager.Instance.Start_Coroutine(LoadAsyncSceneCoroutine(sceneName,loading,complete,setActiveAfterCompleted,Mode));
    }
    private IEnumerator LoadAsyncSceneCoroutine(string sceneName, UnityAction<float> loading = null,
    UnityAction<AsyncOperation> complete = null, bool setActiveAfterCompleted = true, LoadSceneMode Mode = LoadSceneMode.Single)
    {
       AsyncOperation asyncOperation=SceneManager.LoadSceneAsync(sceneName,Mode);
        asyncOperation.allowSceneActivation = false;
        while (asyncOperation.progress<0.9f)
        {
            loading?.Invoke(asyncOperation.progress);
            yield return new WaitForEndOfFrame();
        }
        loading?.Invoke(1f);
        asyncOperation.allowSceneActivation = setActiveAfterCompleted;
        complete?.Invoke(asyncOperation);
    }
    /// <summary>
    ///异步加载当前场景
    /// </summary>
    public void LoadActiveSceneAsync(UnityAction<float> loading = null,UnityAction<AsyncOperation> complete=null,bool setActiveAfterCompleted=true,LoadSceneMode Mode=LoadSceneMode.Single)
    {
        LoadAsyncScene(SceneManager.GetActiveScene().buildIndex,loading,complete,setActiveAfterCompleted,Mode);
    }
    /// <summary>
    /// 异步加载下一个场景
    /// </summary>
    public void LoadNextSceneAsync(bool isCycicle=false, UnityAction<float> loading = null, UnityAction<AsyncOperation> complete = null, bool setActiveAfterCompleted = true, LoadSceneMode Mode = LoadSceneMode.Single)
    {
        int BuildSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (BuildSceneIndex>SceneManager.sceneCountInBuildSettings-1)
        {
            if (isCycicle)
            {
                BuildSceneIndex = 0;
            }
            else
            {
                Debug.Log("超出索引范围");
                return;
            }
        }
        LoadAsyncScene(BuildSceneIndex,loading,complete,false,Mode);
    }
    /// <summary>
    /// 异步加载上一个场景
    /// </summary>
    /// <param name="isCycicle"></param>
    /// <param name="loading"></param>
    /// <param name="complete"></param>
    /// <param name="setActiveAfterCompleted"></param>
    /// <param name="Mode"></param>
    public void LoadPreviewSceneAsync(bool isCycicle = false, UnityAction<float> loading = null, UnityAction<AsyncOperation> complete = null, bool setActiveAfterCompleted = true, LoadSceneMode Mode = LoadSceneMode.Single)
    {
        int BuildSceneIndex = SceneManager.GetActiveScene().buildIndex -1;
        if (BuildSceneIndex <0)
        {
            if (isCycicle)
            {
                BuildSceneIndex = SceneManager.sceneCountInBuildSettings-1;
            }
            else
            {
                Debug.Log("超出索引范围");
                return;
            }
        }
        LoadAsyncScene(BuildSceneIndex, loading, complete, false, Mode);
    }
    #endregion
    #region 异步销毁场景(仅Additive加载的场景需要显示调用卸载方法)
    /// <summary>
    /// 异步销毁场景并卸载无引用对象
    /// </summary>
    /// <param name="BuildIndex"></param>
    /// <param name="callBack"></param>
    /// <param name="options"></param>
    public void DestroySceneAsync(int BuildIndex,UnityAction callBack=null,
        UnloadSceneOptions options= UnloadSceneOptions.None)
    {
        MonoManager.Instance.Start_Coroutine(DestroySceneAsyncCoroutine(BuildIndex,callBack,options));
    }
    private IEnumerator DestroySceneAsyncCoroutine(int BuildIndex, UnityAction callBack = null,
        UnloadSceneOptions options = UnloadSceneOptions.None)
    {
        //异步卸载场景并接受异步操作对象
        AsyncOperation asyncOperation=SceneManager.UnloadSceneAsync(BuildIndex, options);
        //判断能否销毁
        if (asyncOperation==null)
        {
            Debug.LogError("销毁无效");
            yield break;
        }
        while (asyncOperation.progress < 0.9f) yield return new WaitForEndOfFrame();
        callBack?.Invoke();
    }
    #endregion
    /// <summary>
    /// 判断场景索引是否正确
    /// </summary>
    /// <param name="BuildIndex"></param>
    /// <returns></returns>
    private bool isSceneBuildIndexValid(int BuildIndex)
    {
        if (BuildIndex<0)
        {
            Debug.Log("超出索引");
            return false;
        }
        if (BuildIndex>SceneManager.sceneCountInBuildSettings-1)
        {
            Debug.Log("超出索引");
            return false;
        }
        return true;
    }
}
