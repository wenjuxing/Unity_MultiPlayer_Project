using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginDialog
{
    public static void Show(string title, string content, Chibi.Free.Dialog.ActionButton[] buttons)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Chibi.Free.Dialog dialog = GameObject.Find("ChibiDialog").GetComponent<Chibi.Free.Dialog>();
            dialog.ShowDialog(title, content, buttons);
        });
    }
    /// <summary>
    /// 单选响应提示框
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    public static void ShowMessage(string title,string content,System.Action action=null)
    {
        var ok = new Chibi.Free.Dialog.ActionButton("确定", action, new Color(0f, 0.9f, 0.9f));
        Chibi.Free.Dialog.ActionButton[] buttons = { ok };
        Show(title, content, buttons);
    }
    /// <summary>
    /// 双选响应提示框
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="Btn1"></param>
    /// <param name="Btn2"></param>
    /// <param name="action1"></param>
    /// <param name="action2"></param>
    public static void ShowMessage(string title,string content,string Btn1,string Btn2, System.Action action1,System.Action action2=null)
    {
        var ok = new Chibi.Free.Dialog.ActionButton(Btn1, action1, new Color(0f, 0.9f, 0.9f));
        var cancel = new Chibi.Free.Dialog.ActionButton(Btn2);
        Chibi.Free.Dialog.ActionButton[] buttons = { ok, cancel };
        Show(title, content, buttons);
    }
}
