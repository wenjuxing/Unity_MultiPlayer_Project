using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public delegate void OnOKBtnClick(int value);
public delegate void OnCanelBtnClick();
public class NumInputPanel : UIBase
{
    [Header("组件")]
    private Text Title;
    private InputField NumInput;
    private Button AddBtn;
    private Button DelBtn;
    private Button OKBtn;
    private Button CancelBtn;
    [Header("参数")]
    private int currentValue;
    private Action<int> okAction;
    private Action cancelAction;
    public static event OnOKBtnClick onOKBtnClick;
    public static event OnCanelBtnClick onCanelBtnClick;
    private void Start()
    {
        //获取组件
        Title = transform.Find("Title").gameObject.GetComponent<Text>();
        NumInput = transform.Find("NumInput").gameObject.GetComponent<InputField>();
        AddBtn = transform.Find("AddBtn").gameObject.GetComponent<Button>();
        DelBtn = transform.Find("DelBtn").gameObject.GetComponent<Button>();
        OKBtn = transform.Find("OKBtn").gameObject.GetComponent<Button>();
        CancelBtn = transform.Find("CancelBtn").gameObject.GetComponent<Button>();

        //添加事件
        AddBtn.onClick.AddListener(OnAddBtnClick);
        DelBtn.onClick.AddListener(OnDelBtnClick);
        OKBtn.onClick.AddListener(OnOKBtnClick);
        CancelBtn.onClick.AddListener(OnCancelBtnClick);
        currentValue = 0;
        //输入框只能输入数字
        NumInput.contentType = InputField.ContentType.IntegerNumber;
        //更新显示文本
        UpdateDisPlayText();
    }

    private void UpdateDisPlayText()
    {
        NumInput.text = currentValue.ToString();
    }

    private void OnCancelBtnClick()
    {
        onCanelBtnClick?.Invoke();
        base.Hide();
    }

    private void OnOKBtnClick()
    {
        currentValue = int.Parse(NumInput.text);
        onOKBtnClick?.Invoke(currentValue);
        base.Hide();
    }
    private void OnDelBtnClick()
    {
        currentValue--;
        UpdateDisPlayText();
    }

    private void OnAddBtnClick()
    {
        currentValue++;
        UpdateDisPlayText();
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="title"></param>
    /// <param name="value"></param>
    /// <param name="ok"></param>
    /// <param name="cancel"></param>
    public void Show(string title,int value,Action<int> ok=null,Action cancel=null)
    {
        Title.text = title;
        currentValue = value;
        NumInput.text = currentValue.ToString();
        okAction = ok;
        cancelAction = cancel;
        //顶层显示
        transform.SetAsLastSibling();
    }
}
