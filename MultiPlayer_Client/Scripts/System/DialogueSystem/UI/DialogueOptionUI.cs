using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionUI : MonoBehaviour
{
    [SerializeField]private Text text;
    private Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    /// <summary>
    /// 设置选项数据
    /// </summary>
    public void SetData(DialogueOption option,Action action)
    {
        text.text = option.content;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=> { action?.Invoke(); });
    }
}
