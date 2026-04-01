using Proto;
using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleCreatePanel : UIBase
{
    private int SelectedJobId = 1;
    private string[] Jobs = new string[] { "", "战士", "法师", "刺客", "射手" }; //职业类型
    private void Start()
    {
        //注册事件
        OnBtnClick("Panel/ReturnBtn",ToSelectRole);
        OnBtnClick("CreatRoleBtn", CreateRole);
        OnBtnClick("Panel/Button1", SelectJob,1);
        OnBtnClick("Panel/Button2", SelectJob,2);
        OnBtnClick("Panel/Button3", SelectJob,3);
        OnBtnClick("Panel/Button4", SelectJob,4);
        //订阅角色创建响应事件
        MessageRouter.Instance.Subscribe<ChracterCreateResponse>(_ChracterCreateResponse);
        
    }
    /// <summary>
    /// 创建角色响应事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _ChracterCreateResponse(Connection conn, ChracterCreateResponse msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var tipPanel = (UIManager.Instance.ShowUI<TipType1Panel>
            (E_UIPanelLayer.Forefront)) as TipType1Panel;
            //创建成功
            if (msg.Success)
            {
                tipPanel.Show("系统消息", "创建成功", () =>
            {

              //创建角色成功后刷新角色列表
              CharacterListRequest resp = new CharacterListRequest();
              NetClient.Send(resp);
              //返回角色选择列表
              ToSelectRole();
              UIManager.Instance.HideUI("TipType1Panel");
                });
            }
            //数量超额
            else
            {
                tipPanel.Show("系统消息", "数量超额", () =>
                {UIManager.Instance.HideUI("TipType1Panel");});
            }

        });
    }
    /// <summary>
    /// 选择职业
    /// </summary>
    /// <param name="jobId"></param>
    public void SelectJob(int jobId)
    {
        SelectedJobId = jobId;
        GameObject.Find("RoleCreatePanel/SelectedRole/JobText").GetComponent<Text>().text = Jobs[jobId];
    }
    /// <summary>
    /// 创建角色
    /// </summary>
    public void CreateRole()
    {
        var name = GameObject.Find("RoleCreatePanel/InputField").GetComponent<InputField>().text;
        Debug.Log($"角色:{Jobs[SelectedJobId]}，名字:{name}");
        //设置角色信息并发送网络请求
        CharacterCreateRequest req = new CharacterCreateRequest();
        req.Name = name;
        req.JobType = SelectedJobId;
        NetClient.Send(req);
    }
    /// <summary>
    /// 去选择角色
    /// </summary>
    private void ToSelectRole()
    {
        UIManager.Instance.ShowUI<RoleSelectPanel>();
        base.Hide();
    }
}
