using GameServer.Battle;
using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
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
            int job = this.Owner.Define.TID;
            if (job==1)
            {
                LoadSkill(1001, 1002, 1003);
            }
            if (job == 2)
            {
                LoadSkill(2001,2002);
            }
            if (job==1002||job==1003)
            {
                LoadSkill(101);
            }
        }
        /// <summary>
        /// 加载技能
        /// </summary>
        private void LoadSkill(params int[] ids)
        {
            foreach (int skid in ids)
            {
                Owner.info.Skills.Add(new Proto.SkillInfo() {Id=skid });
                var skill = new Skill(this.Owner,skid);
                Skills.Add(skill);
                Log.Information("角色:{0},技能ID:{1},技能名称:{2}", Owner.Name, skill.Define.ID, skill.Define.Name);
                
            }
        }
        public Skill GetSkill(int id)
        {
            return Skills.FirstOrDefault(s=>s.Define.ID==id);
        }
        /// <summary>
        /// 更新技能状态
        /// </summary>
        public void Update()
        {
            //遍历技能列表
            Skills.ForEach(skill=>skill.Update());
        }
    }
}
