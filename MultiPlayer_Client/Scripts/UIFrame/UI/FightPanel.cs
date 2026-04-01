using Assets.Scripts.U3d_scripts;
using GameClient;
using GameClient.Battle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FightPanel : UIBase
{
    [HideInInspector]public Slider IntonateSlider;
    private UnitFrame playerFrame;
    private UnitFrame targetFrame;
   
    private bool IsShow;
    private void Awake()
    {
        IntonateSlider = transform.Find("IntonateSlider").gameObject.GetComponent<Slider>();
        playerFrame = transform.Find("PlayerFrame").gameObject.GetComponent<UnitFrame>();
        targetFrame = transform.Find("TargetFrame").gameObject.GetComponent<UnitFrame>();
    }
    private void Update()
    {
        //设置角色头像框的所属者
        playerFrame.actor = GameApp.character;
        targetFrame.actor = GameApp.Target;
        //显示玩法面板
        if (GameApp.character != null&&!IsShow)
        {
            UIManager.Instance.ShowUI<PlayPanel>();
            IsShow = true;
        }

       var skill=GameApp.CurrSkill;
        if (skill !=null&& skill.State==SkillState.Casting&&skill.Define.IntonateTime>0.1f)
        {
            IntonateSlider.gameObject.SetActive(true);
            IntonateSlider.value = skill.IntonateProgress;
        }
        else
        {
            IntonateSlider.gameObject.SetActive(false);
        }
        AutoSelectTarget();
    }
    /// <summary>
    /// 自动选择攻击目标
    /// </summary>
    private void AutoSelectTarget()
    {
        //选择目标快捷键
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameApp.SelectTarget();
        }
        //目标死亡清空目标
        if (GameApp.Target != null && GameApp.Target.IsDeath)
        {
            GameApp.Target = null;
        }
    }
    /// <summary>
    /// 暴击震屏效果
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="amount"></param>
    public  void ShakeScreen(float duration=0.5f,float amount=0.1f)
    {
        Camera.main.DOShakePosition(duration,amount);
    }
}
