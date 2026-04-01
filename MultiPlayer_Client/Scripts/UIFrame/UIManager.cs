using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// UI面板层枚举
/// </summary>
public enum E_UIPanelLayer
{
    None,
    Rearmost,//最后方
    Rear,//后方
    Middle,//中间
    Front,//前方
    Forefront//最前方
}

public class UIManager : SingletonBase<UIManager>
{
    //记录每层的父物体
    private Dictionary<E_UIPanelLayer, Transform> layerParents;
    //存储加载过界面的集合（里氏替换原则父类容器装子类对象）
    private List<UIBase> uiList = new List<UIBase>();
    private void Awake()
    {
        //初始化
        CreateCanvas();
        CreateLayer();
        CreateEventSystem();
    }
    /// <summary>
    /// 创建画布
    /// </summary>
    private void CreateCanvas()
    {
        //设置层级
        gameObject.layer = LayerMask.NameToLayer("UI");
        //设置Canvas组件
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3000;
        //设置CanvasScaler组件
        CanvasScaler canvasScaler=gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920,1080);
        //横版屏幕设置1，竖版屏幕设置0
        canvasScaler.matchWidthOrHeight = Screen.width > Screen.height ? 1 : 0;
        //设置GraphicRaycaster组件
        gameObject.AddComponent<GraphicRaycaster>();
    }
    /// <summary>
    /// 创建父物体层
    /// </summary>
    private void CreateLayer()
    {
        //设置Rearmost层父对象
        Transform rearmost = new GameObject(E_UIPanelLayer.Rearmost.ToString()).transform;
        rearmost.SetParent(transform,false);

        //设置Rear层父对象
        Transform rear = new GameObject(E_UIPanelLayer.Rear.ToString()).transform;
        rear.SetParent(transform, false);

        //设置Middle层父对象
        Transform middle = new GameObject(E_UIPanelLayer.Middle.ToString()).transform;
        middle.SetParent(transform, false);

        //设置Front层父对象
        Transform front = new GameObject(E_UIPanelLayer.Front.ToString()).transform;
        front.SetParent(transform, false);

        //设置Forefront层父对象
        Transform forefront = new GameObject(E_UIPanelLayer.Forefront.ToString()).transform;
        forefront.SetParent(transform, false);

        //记录每个层的父物体
        layerParents = new Dictionary<E_UIPanelLayer, Transform>()
        {
            {E_UIPanelLayer.Rearmost,rearmost },
            {E_UIPanelLayer.Rear,rear },
            {E_UIPanelLayer.Middle,middle },
            {E_UIPanelLayer.Front,front },
            {E_UIPanelLayer.Forefront,forefront },
        };
    }
    /// <summary>
    /// 创建事件系统
    /// </summary>
    private void CreateEventSystem()
    {
        //事件系统存在？
        if (FindObjectOfType<EventSystem>()) return;
        GameObject eventSystem = new GameObject("EventSystem");
        DontDestroyOnLoad(eventSystem);
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
    /// <summary>
    /// 显示面板
    /// </summary>
    /// <returns></returns>
    public UIBase ShowUI<T>(E_UIPanelLayer layer=E_UIPanelLayer.Middle,bool doTween=true) where T:UIBase
    {
        DOTween.CompleteAll(true);
        //获取名称
        string uiName = typeof(T).Name;
        //获取面板
        UIBase ui = Find(uiName);
        //ui存在？
        if (ui==null)
        {
            Transform parent = layerParents[layer];
            Debug.Log("UI/Panel/" + uiName);
            GameObject obj = Instantiate(Resources.Load("Prefabs/UI/Panel/" + uiName), parent) as GameObject;
            obj.name = uiName;
            ui = obj.AddComponent<T>();
            obj.AddComponent<CanvasGroup>();
            uiList.Add(ui);
        }
        else
        {
            ui.Show();
        }
        //设置透明度
        CanvasGroup canvasGroup = ui.transform.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        //动画
        if (doTween) ui.transform.DOScale(Vector3.one,0.5f).From(Vector3.zero);
        return ui;
    }
    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="name"></param>
    public void HideUI(string uiName,bool doTween=true)
    {
        //强制完成所有动画
        DOTween.CompleteAll(true);
        //ui存在？
        UIBase ui = Find(uiName);
        if (ui==null) return;
        //动画
        if (doTween)
        {
            CanvasGroup canvasGroup = ui.transform.GetComponent<CanvasGroup>();
            //创建序列对象
            Sequence closeSequeue = DOTween.Sequence();
            //淡出 移动 隐藏
            closeSequeue.Append(canvasGroup.DOFade(0, 0.8f))
                .OnComplete(() => { ui.gameObject.SetActive(false); });
        }
        else
        {
            //隐藏
            ui.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name"></param>
    public void CloseUI(string uiName,bool doTween=true)
    {
        //强制完成所有动画
        DOTween.CompleteAll(true);
        //ui存在？
        UIBase ui = Find(uiName);
        if (ui == null) return;
        //动画
        if (doTween)
        {
            CanvasGroup canvasGroup = ui.transform.GetComponent<CanvasGroup>();
            //创建序列对象
            Sequence closeSequeue = DOTween.Sequence();
            //淡出 移动 移除列表 销毁对象
            closeSequeue.Append(canvasGroup.DOFade(0, 0.8f))
                .OnComplete(() => 
                {
                    uiList.Remove(ui);
                    Destroy(ui.gameObject);
                }) ;
        }
        else
        {
            uiList.Remove(ui);
            Destroy(ui.gameObject);
        }
    }
    /// <summary>
    /// 关闭所有界面
    /// </summary>
    public void CloseAllUI()
    {
        foreach (var ui in uiList)
        {
            Destroy(ui);
        }
        uiList.Clear();
    }
    /// <summary>
    /// 查找加载的面板
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    private UIBase Find(string uiName)
    {
        foreach (var ui in uiList)
        {
            if (ui.name == uiName) return ui;
        }
        return null;
    }
    /// <summary>
    /// 给外界提供获取面板的方法
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public T Find<T>()where T:UIBase
    {
        //获取名称
        string uiName = typeof(T).Name;
        //获取面板
        var ui = Find(uiName)as T;
        if (ui != null) return ui;
        return null;
    }
    /// <summary>
    /// 提示界面
    /// </summary>
    public void ShowTips(string msg,Color color,float showTime=0.5f,UnityAction callBack=null,E_UIPanelLayer layer=E_UIPanelLayer.Forefront)
    {
        DOTween.CompleteAll(true);
        //
        UIBase ui = ShowUI<TipType1Panel>(layer,false);
        //查找文本组件并设置内容
        TextMeshProUGUI text = ui.transform.Find("bg/text").GetComponent<TextMeshProUGUI>();
        text.color = color;
        text.text = msg;
        //动画
        Sequence sequence = DOTween.Sequence();
        //0缩放至1 先显示 延迟n秒后1缩放至0
        sequence.Append(ui.transform.DOScaleY(1, 0.4f).From(0))
            .Append(DOVirtual.DelayedCall(showTime, () => { }))
            .Append(ui.transform.DOScaleY(0, 0.4f).From(1))
            .OnComplete(() =>
            {
                ui.gameObject.SetActive(false);
                callBack?.Invoke();
            });
    }
}
