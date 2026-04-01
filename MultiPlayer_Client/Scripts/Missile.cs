using GameClient.Battle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    //所属技能
    public Skill skill { get; private set; }
    //追击目标
    public GameObject Target { get; private set; }
    //初始位置
    public Vector3 InitPos { get; private set; }
    //特效
    private GameObject child;
    /// <summary>
    /// 初始化特效
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="InitPos"></param>
    /// <param name="target"></param>
    public void Init(Skill skill,Vector3 InitPos,GameObject target)
    {
        this.skill = skill;
        this.InitPos = InitPos;
        this.Target = target;
        transform.position = InitPos;
        var prefab = Resources.Load<GameObject>(skill.Define.Missile);
        if (prefab!=null)
        {
            child = Instantiate(prefab,Vector3.zero,Quaternion.identity,transform);
        }
    }
    private void Start()
    {
        //设置初始大小
        transform.localScale = Vector3.one * 0.1f;
    }
    private void FixedUpdate()
    {
        OnUpdate(Time.fixedDeltaTime);
    }

    private void OnUpdate(float dt)
    {
        var a = transform.position;
        var b = Target.transform.position;
        Vector3 dir = (b - a).normalized;
        float dict = skill.Define.MissileSpeed * 0.001f * dt;
        if (dict>=Vector3.Distance(b,a))
        {
            transform.position = b;
            Destroy(this.gameObject,0.1f);
        }
        else
        {
            transform.position += dir * dict;
        }
        child.transform.localPosition = Vector3.zero;
    }
}
