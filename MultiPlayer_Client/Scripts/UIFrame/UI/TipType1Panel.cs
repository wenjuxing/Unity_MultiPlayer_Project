using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
public class TipType1Panel : UIBase
{
    private Text Title;
    private TextMeshProUGUI Content;
    private Button ConfirmBtn;
    private void Awake()
    {
        Title = transform.Find("Panel/Title").gameObject.GetComponent<Text>();
        Content = transform.Find("Panel/Content").gameObject.GetComponent<TextMeshProUGUI>();
        ConfirmBtn = transform.Find("Panel/ConfirmBtn")?.GetComponent<Button>();
    }
    public void Show(string title,string content,UnityAction action)
    {
        //清空上次点击注册的事件，避免重复调用事件
        ConfirmBtn?.onClick.RemoveAllListeners();

        Title.text = title;
        Content.text = content;
        ConfirmBtn.onClick.AddListener(action);
    }
    /// <summary>
    /// 注销事件
    /// </summary>
    private void OnDisable()
    {
        ConfirmBtn?.onClick.RemoveAllListeners();
    }
    private void OnDestroy()
    {
        ConfirmBtn?.onClick.RemoveAllListeners();
    }
}
