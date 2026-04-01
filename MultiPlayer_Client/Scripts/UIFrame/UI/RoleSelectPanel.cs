using Assets.Scripts;
using Proto;
using Summer.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 角色信息
/// </summary>
public class RoleSelectPanel : UIBase
{
    private List<GameObject> rolePanelList = new List<GameObject>();  //角色面板列表
    private List<RoleInfo> roleList = new List<RoleInfo>();           //角色列表
    private string[] Jobs = new string[] { "", "战士", "法师", "刺客", "射手" }; //职业类型
    private int SelectedIndex = -1;
    private void Start()
    {
        //注册事件
        OnBtnClick("RoleListPanel/DelBtn",DeleteRole);
        OnBtnClick("RoleListPanel/CreateBtn", ToCreateRole);
        OnBtnClick("CreatRolePanel/EnterGameBtn", EnterGame);
        OnBtnClick("RoleListPanel/HeroPanel (0)", RoleClick,0);
        OnBtnClick("RoleListPanel/HeroPanel (1)", RoleClick,1);
        OnBtnClick("RoleListPanel/HeroPanel (2)", RoleClick,2);
        OnBtnClick("RoleListPanel/HeroPanel (3)", RoleClick,3);
        MessageRouter.Instance.Subscribe<CharacterListResponse>(_CharacterListResponse);
        MessageRouter.Instance.Subscribe<CharacterDeleteResponse>(_CharacterDeleteResponse);

        for (int i = 0; i < 4; i++)
        {
            rolePanelList.Add(GameObject.Find($"RoleSelectPanel/RoleListPanel/HeroPanel ({i})"));
        }
        //隐藏默认的面板数据
        foreach (var go in rolePanelList) go.SetActive(false);
        //请求角色列表
        CharacterListRequest resp = new CharacterListRequest();
        NetClient.Send(resp);
    }
    /// <summary>
    /// 角色列表的响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _CharacterListResponse(Connection sender, CharacterListResponse msg)
    {
        Debug.Log("角色列表" + msg);
        roleList.Clear();
        //遍历网络申请的角色列表赋值到角色列表中
        foreach (var c in msg.ActorList)
        {
            roleList.Add(new RoleInfo() { Name = c.Name, Job = c.Tid, Level = c.Level, RoleId = c.Id });
        }
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            LoadRoleList();
        });

    }
    /// <summary>
    /// 角色删除响应事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msg"></param>
    private void _CharacterDeleteResponse(Connection conn, CharacterDeleteResponse msg)
    {
        //重新加载角色列表
        CharacterListRequest resp = new CharacterListRequest();
        NetClient.Send(resp);
    }
    /// <summary>
    /// 点击角色标记
    /// </summary>
    /// <param name="num"></param>
    public void RoleClick(int num)
    {
        //
        SelectedIndex = num;
        //通过标记获取列表中的角色信息
        RoleInfo roleInfo = roleList[num];
        //
        var pn2 = GameObject.Find("RoleSelectPanel/RoleInfoPanel");
        pn2.transform.Find("Name/NameText").GetComponent<Text>().text = roleInfo.Name;
        pn2.transform.Find("Job/NameText").GetComponent<Text>().text = Jobs[roleInfo.Job];
        pn2.transform.Find("Level/NameText").GetComponent<Text>().text = roleInfo.Level.ToString();

        //高亮显示被选择的按钮
        for (int i = 0; i < rolePanelList.Count; i++)
        {
            rolePanelList[i].transform.Find("Image").gameObject.SetActive(i == num);
        }
    }
    /// <summary>
    /// 删除角色
    /// </summary>
    public void DeleteRole()
    {
        if (SelectedIndex < 0) return;
        var role = roleList[SelectedIndex];
        Debug.Log($"删除:角色Id:{role.RoleId},角色名称:{role.Name}");
        //发送删除请求
        ((UIManager.Instance.ShowUI<TipType2Panel>(E_UIPanelLayer.Forefront))
              as TipType2Panel)
              .Show("系统消息", "确定删除?",
              ()=> 
              {
                  CharacterDeleteRequest delResp = new CharacterDeleteRequest();
                  delResp.CharacterId = role.RoleId;
                  NetClient.Send(delResp);
                  UIManager.Instance.HideUI("TipType2Panel");
              },
              ()=> 
              {
                  UIManager.Instance.HideUI("TipType2Panel");
              });
    }
    /// <summary>
    /// 获取角色列表信息
    /// </summary>
    public void LoadRoleList()
    {
        foreach (var go in rolePanelList) go.SetActive(false);
        for (int i = 0; i < roleList.Count; i++)
        {
            rolePanelList[i].SetActive(true);
            rolePanelList[i].transform.Find("Text_Name").GetComponent<Text>().text = roleList[i].Name;
            rolePanelList[i].transform.Find("Text_Job").GetComponent<Text>().text = Jobs[roleList[i].Job];
            rolePanelList[i].transform.Find("Text_Level").GetComponent<Text>().text = roleList[i].Level.ToString();
        }
    }
    /// <summary>
    /// 进入游戏
    /// </summary>
    public void EnterGame()
    {
        if (SelectedIndex < 0) return;
        var role = roleList[SelectedIndex];
        //Debug.Log($"进入游戏:{role.Name}");
        Kaiyun.Event.FireIn("EnterGame", role.RoleId);
        //关闭并销毁
        base.Close();
    }
    /// <summary>
    /// 去创建角色
    /// </summary>
    private void ToCreateRole()
    {
        base.Hide();
        UIManager.Instance.ShowUI<RoleCreatePanel>();
    }
}
