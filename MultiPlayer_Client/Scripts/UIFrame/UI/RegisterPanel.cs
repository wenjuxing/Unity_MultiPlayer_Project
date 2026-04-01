using Assets.Scripts.U3d_scripts;
using Proto;
using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterPanel : UIBase
{
    private InputField UserName;
    private InputField Password;
    private Button LoginBtn;
    private Button RegisterBtn;
    void Start()
    {
        //获取组件
        UserName = transform.Find("Panel/UserNameInput").gameObject.GetComponent<InputField>();
        Password = transform.Find("Panel/PasswordInput").gameObject.GetComponent<InputField>();
        //注册事件
        OnBtnClick("Panel/LoginBtn", ToLogin);
        OnBtnClick("Panel/RegisterBtn", RegisterRequest);

        MessageRouter.Instance.Subscribe<UserRegisterResponse>(_UserRegisterResponse);
    }
    /// <summary>
    /// 去登录
    /// </summary>
    private void ToLogin()
    {
        base.Hide();
        UIManager.Instance.ShowUI<LoginPanel>();
    }
    /// <summary>
    /// 注册响应
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    private void _UserRegisterResponse(Connection conn, UserRegisterResponse msg)
    {
        Debug.Log(msg);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //显示面板 获取UIBase转换类型 调用方法
            ((UIManager.Instance.ShowUI<TipType1Panel>(E_UIPanelLayer.Forefront))
            as TipType1Panel).
           Show("系统消息", "注册成功", () =>
           {
               UIManager.Instance.HideUI("TipType1Panel");
           });
        });
    }
    /// <summary>
    /// 注册请求
    /// </summary>
    private void RegisterRequest()
    {
        //设置注册消息类型
        var msg = new UserRegisterRequest()
        {
            Username = UserName.text,
            Password = Password.text
        };
        NetClient.Send(msg);
    }
}
