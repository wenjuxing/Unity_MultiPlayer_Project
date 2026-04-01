using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneTest : MonoBehaviour
{
    AsyncOperation asyncoperation;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(700, 250, 220, 60), "异步加载场景"))
        {
            LoadSceneManager.Instance.LoadAsyncScene("AsyncScene_Test", loading: progress => { Debug.Log($"加载进度:{progress}"); },
                complete:operation=> { asyncoperation = operation; }, false);
        }
        if (GUI.Button(new Rect(700, 350, 220, 60), "切换场景"))
        {
            asyncoperation.allowSceneActivation = true;
        }
        if (GUI.Button(new Rect(700, 450, 220, 60), "异步加载当前场景"))
        {
            LoadSceneManager.Instance.LoadActiveSceneAsync(loading: progress => { Debug.Log($"加载进程{progress}"); }, complete: operation => { asyncoperation = operation; });
        }
        if (GUI.Button(new Rect(700, 550, 220, 60), "异步加载下一个场景"))
        {
            LoadSceneManager.Instance.LoadNextSceneAsync(false, loading => { }, complete:operation => { asyncoperation = operation; }, false);
        }
        if (GUI.Button(new Rect(700, 650, 220, 60), "切换下一个场景"))
        {
            asyncoperation.allowSceneActivation = true;
        }
        if (GUI.Button(new Rect(700, 750, 220, 60), "异步加载上一个场景"))
        {
            LoadSceneManager.Instance.LoadPreviewSceneAsync(false, loading => { }, complete: operation => { asyncoperation = operation; }, false);
        }
        if (GUI.Button(new Rect(700, 850, 220, 60), "切换上一个场景"))
        {
            asyncoperation.allowSceneActivation = true;
        }
    }
}
