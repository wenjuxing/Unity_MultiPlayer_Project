using GameClient.Mgr;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.Entities
{
    public class Actor : Entity
    {
        public NetActor Info;
        public UnitDefine define;
        public SkillManager SkillMgr;
        //实体对应的游戏对象
        public GameObject renderObj;
        //角色状态
        public UnitState UnitState;
        public bool IsDeath => UnitState == UnitState.Dead;
        public Actor(NetActor info) : base(info.Entity)
        {
            this.Info = info;
            if (info.EntityType!=EntityType.Item)
            {
                this.define = DataManager.Instance.Units[info.Tid];
                this.SkillMgr = new SkillManager(this);
            }
        }

        public void recvDamage(Damage item)
        {
            var _txtPos = renderObj.transform.position + Vector3.up * 2;
            //暴击？
            if (item.IsCrit)
            {
                UIManager.Instance.Find<FightPanel>().ShakeScreen();
                //伤害飘字
                DynamicTextManager.CreateText(_txtPos, item.Amount.ToString("0"),DynamicTextManager.critData);
            }
            else if (item.IsMiss)
            {
                //伤害飘字
                DynamicTextManager.CreateText(_txtPos, "Miss",DynamicTextManager.missData);
            }
            else
            {
                //伤害飘字
                DynamicTextManager.CreateText(_txtPos, item.Amount.ToString("0"));
            }
            //加载受击特效
            var attacker = Game.GetUnit(item.AttackerId);
            var skill = attacker.SkillMgr.GetSkill(item.SkillId);
            var prefab = Resources.Load<ParticleSystem>(skill.Define.HitArt);
            if (prefab != null)
            {
                var pos = renderObj.transform.position + Vector3.up;
                var dir = renderObj.transform.rotation;
                ParticleSystem newPs = GameObject.Instantiate(prefab, pos, dir);
                newPs.Play();
                GameObject.Destroy(newPs.gameObject, newPs.main.duration);
            }
            else
            {
                Debug.LogError("粒子预制件为空!");
            }
        }
        /// <summary>
        /// 血量发生改变
        /// </summary>
        /// <param name="floatValue1"></param>
        /// <param name="floatValue2"></param>
        public virtual void OnHpChanged(float old_hp, float new_hp)
        {
            Info.Hp = new_hp;
        }
        /// <summary>
        /// 蓝量发生改变
        /// </summary>
        /// <param name="floatValue1"></param>
        /// <param name="floatValue2"></param>
        public virtual void OnMpChanged(float old_mp, float new_mp)
        {
            this.Info.Mp = new_mp;
        }
        /// <summary>
        /// 状态发生改变
        /// </summary>
        /// <param name="stateValue1"></param>
        /// <param name="stateValue2"></param>
        public virtual void OnStateChanged(UnitState old_state, UnitState new_state)
        {
            this.UnitState = new_state;
            if (IsDeath)
            {
                Debug.Log("即将死亡");
                if (renderObj == null) return;
                var ani = renderObj.GetComponent<HeroAnimations>();
                ani.PlayDie();
                Game.StartCoroutine(_HideElement());
            }
            else
            {
                renderObj?.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏角色
        /// </summary>
        /// <returns></returns>
        IEnumerator _HideElement()
        {
            yield return new WaitForSeconds(3f);
            if (IsDeath)
            {
                renderObj.SetActive(false);
            }
        }

        
    }
}
