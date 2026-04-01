using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipType2Panel : UIBase
{
    private Text Title;
    private Text Content;
    private Button ConfirmBtn;
    private Button CancelBtn;
    private void Awake()
    {
        Title = transform.Find("Panel/Title").gameObject.GetComponent<Text>();
        Content = transform.Find("Panel/Content").gameObject.GetComponent<Text>();
        ConfirmBtn = transform.Find("Panel/ConfirmBtn")?.GetComponent<Button>();
        CancelBtn = transform.Find("Panel/CancelBtn")?.GetComponent<Button>();
    }
    public void Show(string title, string content, UnityAction confirm, UnityAction cancel)
    {
        //清空上次点击注册的事件，避免重复调用事件
        ConfirmBtn?.onClick.RemoveAllListeners();
        CancelBtn?.onClick.RemoveAllListeners();

        Title.text = title;
        Content.text = content;
        ConfirmBtn.onClick.AddListener(confirm);
        CancelBtn.onClick.AddListener(cancel);
    }
    /// <summary>
    /// 注销事件
    /// </summary>
    private void OnDisable()
    {
        ConfirmBtn?.onClick.RemoveAllListeners();
        CancelBtn?.onClick.RemoveAllListeners();
    }
    private void OnDestroy()
    {
        ConfirmBtn?.onClick.RemoveAllListeners();
        CancelBtn?.onClick.RemoveAllListeners();
    }
}
