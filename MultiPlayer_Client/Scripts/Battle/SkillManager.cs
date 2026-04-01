using GameClient.Battle;
using GameClient.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.Mgr
{
    /// <summary>
    /// 每一个Actor身上都会有一个独立的技能管理器
    /// </summary>
    public class SkillManager
    {
        //技能归属者
        public Actor Owner;
        //技能列表
        public List<Skill> Skills = new List<Skill>();
        public SkillManager(Actor owner)
        {
            this.Owner = owner;
            this.InitSkills();
        }
        /// <summary>
        /// 初始化技能列表
        /// </summary>
        public void InitSkills()
        {
            foreach (var define in Owner.Info.Skills)
            {
                var skill = new Skill(this.Owner,define.Id);
                Skills.Add(skill);
                Debug.Log($"角色:{Owner.define.Name},技能ID:{skill.Define.ID},技能名称:{skill.Define.Name}");
            }
        }
        public void OnUpdate(float delta)
        {
            //遍历技能列表
            foreach (Skill skill in Skills)
            {
                skill.OnUpdate(delta);
            }
        }
        /// <summary>
        /// 获取技能
        /// </summary>
        /// <param name="skillId"></param>
        public Skill GetSkill(int skillId)
        {
            return Skills.FirstOrDefault(s => s.Define.ID == skillId);
        }
    }
}
