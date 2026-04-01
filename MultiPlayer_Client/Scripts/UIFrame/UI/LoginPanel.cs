using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proto;
using Summer.Network;
using System;
using UnityEngine.SceneManagement;
using Assets.Scripts.U3d_scripts;

public class LoginPanel : UIBase
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
        OnBtnClick("Panel/LoginBtn", LoginRequest);
        OnBtnClick("Panel/RegisterBtn", ToRegister);

        MessageRouter.Instance.Subscribe<UserLoginResponse>(_UserLoginResponse);
    }
    /// <summary>
    /// 去注册
    /// </summary>
    private void ToRegister()
    {
        base.Hide();
        UIManager.Instance.ShowUI<RegisterPanel>();
    }
    /// <summary>
    /// 登录响应
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="msg"></param>
    private void _UserLoginResponse(Connection conn, UserLoginResponse msg)
    {
        Debug.Log(msg);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //显示面板 获取UIBase转换类型 调用方法
            ((UIManager.Instance.ShowUI<TipType1Panel>(E_UIPanelLayer.Forefront))
            as TipType1Panel).
           Show("系统消息", "登录成功", () =>
            {
                //登录成功切换到选择角色场景
                if (msg.Success)
                {
                    UIManager.Instance.HideUI("TipType1Panel");
                    SceneManager.LoadSceneAsync("SelectCharacter");
                    UIManager.Instance.ShowUI<RoleSelectPanel>();
                    base.Close();
                }
            });
        });
        GameApp.playerId = msg.PlayerId;
    }
    /// <summary>
    /// 登录请求
    /// </summary>
    private void LoginRequest()
    {
        //设置登录消息类型
        var msg = new UserLoginRequest()
        {
            Username = UserName.text,
            Password = Password.text
        };
        NetClient.Send(msg);
    }
}
