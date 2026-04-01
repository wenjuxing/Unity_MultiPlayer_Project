using Assets.Scripts.U3d_scripts;
using GameClient.Entities;
using GameServer.Fight;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.Battle
{
    //开始--蓄力--激活--结束
    public enum SkillState
    {
        None,
        Casting,
        Active,
        Coolding
    }
    public class Skill
    {
        public SkillDefine Define;   //技能设定
        public Actor Owner;          //技能所属者
        public float Cooldown;       //冷却计时
        private float _time;        //技能运行时间
        public SkillState State;     //当前技能状态 
        private Sprite _icon;
        private SCObject _sco;       //技能目标

        public bool IsUnitTarget { get => Define.TargetType == "单位"; }
        public bool IsPointTarget { get => Define.TargetType == "点"; }
        public bool IsNoneTarget { get => Define.TargetType == "None"; }
        public float IntonateProgress => _time / Define.IntonateTime;

        public Sprite Icon { 
            get {
                if (_icon == null)
                {
                    _icon = Resources.Load<Sprite>(Define.Icon);
                }
                return _icon;
            }
        }
        public Skill(Actor owner,int skillId)
        {
            this.Owner = owner;
            Debug.Log(DataManager.Instance.Skills[skillId]+"获取技能");
            Define = DataManager.Instance.Skills[skillId];
        }
        /// <summary>
        /// 更新技能状态
        /// </summary>
        /// <param name="delta"></param>
        public void OnUpdate(float delta)
        {
            if (State == SkillState.None && Cooldown == 0) return;

            //计算冷却时间
            if (Cooldown > 0) Cooldown -= Time.deltaTime;
            if (Cooldown < 0) Cooldown = 0;

            //技能运行计时
            _time += Time.deltaTime;

            //蓄力状态=>激活状态
            if (State == SkillState.Casting && _time >= Define.IntonateTime)
            {
                State = SkillState.Active;
                Cooldown = Define.CD;
                OnActive();
                Debug.Log("技能状态：蓄力状态=>激活状态");
            }
            //激活状态=>冷却状态
            if (State == SkillState.Active)
            {
                if (_time >= Define.IntonateTime + Define.Duration)
                {
                    State = SkillState.Coolding;
                    Debug.Log("技能状态：激活状态=>冷却状态");
                }
            }
            //冷却状态=>无状态
            if (State == SkillState.Coolding)
            {
                if (Cooldown == 0)
                {
                    _time = 0;
                    State = SkillState.None;
                    OnFinish();
                    Debug.Log("技能状态：冷却状态=>无状态");
                }
            }
        }
        /// <summary>
        /// 技能激活时调用
        /// </summary>
        private void OnActive()
        {
            Log.Information("技能激活：Skill[{0}],Owner[{1}]", Define.Name, Owner.entityId);
            Kaiyun.Event.FireOut("OnSkillActive", this);
            //飞行特效？
            if (Define.IsMissile)
            {
                var target = _sco.RealObj as Actor;
                GameObject myObject = new GameObject("Missile");
               var missile=myObject.AddComponent<Missile>();
                missile.Init(this,Owner.renderObj.transform.position,target.renderObj);
            }
        }
        /// <summary>
        /// 命中特效
        /// </summary>
        private void OnHit()
        {
            //加载特效
            var ps = Resources.Load<ParticleSystem>(Define.HitArt);
            if (ps != null)
            {
                if (_sco is SCEntity)
                {
                    var target = _sco.RealObj as Actor;
                    var pos = target.renderObj.transform.position + Vector3.up * 0.9f;
                    var dir = target.renderObj.transform.rotation;
                    ParticleSystem newPs = GameObject.Instantiate(ps, pos, dir);
                    newPs.Play();
                    GameObject.Destroy(newPs.gameObject, newPs.main.duration);
                }
                if (_sco is SCPosition)
                {
                    ParticleSystem newPs = GameObject.Instantiate(ps, _sco.Position, Quaternion.identity);
                    newPs.Play();
                    GameObject.Destroy(newPs.gameObject, newPs.main.duration);
                }
            }
        }
        /// <summary>
        /// 技能结束
        /// </summary>
        private void OnFinish()
        {
            Log.Information("技能结束：Skill[{0}],Owner[{1}]", Define.Name, Owner.entityId);
        }
        /// <summary>
        /// 开始蓄力
        /// </summary>
        private void OnIntonate()
        {
            //在主线程中使得角色朝向目标
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (_sco is SCEntity)
                {
                    var actor = _sco.RealObj as Actor;
                    Owner.renderObj.transform.LookAt(actor.renderObj.transform);
                }
            });
            Kaiyun.Event.FireOut("OnSkillIntonate",this);
        }
        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="sco"></param>
        /// <returns></returns>
        public void Use(SCObject sco)
        {
            //技能所属者等于当前玩家？
            if (Owner.entityId == GameApp.character.entityId)
            {
                GameApp.CurrSkill = this;
            }
            _time = 0;
            _sco = sco;
            //进入技能蓄力状态
            State = SkillState.Casting;
            OnIntonate();
        }
    }
}
