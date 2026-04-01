using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    /// <summary>
    /// 显示
    /// </summary>
    public virtual void Show() 
    {
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);
    }
    /// <summary>
    /// 隐藏
    /// </summary>
    public virtual void Hide()
    {
        UIManager.Instance.HideUI(gameObject.name);
    }
    /// <summary>
    /// 关闭
    /// </summary>
    public virtual void Close()
    {
        UIManager.Instance.CloseUI(gameObject.name);
    }
    /// <summary>
    /// 封装点击事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onBtnClick"></param>
    public void OnBtnClick(string name,UnityAction onBtnClick)
    {
        transform.Find(name)?.
            GetComponent<Button>()?.
            onClick.AddListener(() => onBtnClick?.Invoke());
    }
    public void OnBtnClick(string name, UnityAction<int> onBtnClick,int data)
    {
        transform.Find(name)?.
            GetComponent<Button>()?.
            onClick.AddListener(() => onBtnClick?.Invoke(data));
    }
    /// <summary>
    /// 封装滑动条事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onSliderChange"></param>
    public void OnSliderChanged(string name,UnityAction<float> onSliderChange)
    {
        transform.Find(name)?.
            GetComponent<Slider>()?.
            onValueChanged.AddListener(value => onSliderChange?.Invoke(value));
    }
    /// <summary>
    /// 封装滚动视图事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onSliderChange"></param>
    public void OnScrollViewChanged(string name, UnityAction onScrollViewChanged)
    {
        transform.Find(name)?.
            GetComponent<ScrollRect>()?.
            onValueChanged.AddListener(value => onScrollViewChanged?.Invoke());
    }
    /// <summary>
    /// 封装切换事件
    /// </summary>
    public void OnToggleClick(string name,UnityAction<bool> onToggleClick)
    {
        transform.Find(name)?.
            GetComponent<Toggle>()?.
            onValueChanged.AddListener(value=>onToggleClick?.Invoke(value));
    }
}
