using GameServer.Core;
using GameServer.Model;
using Proto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
    public class Spell
    {
        public Actor Owner { get; set; }
        public Spell(Actor owner)
        {
            this.Owner = owner;
        }
        /// <summary>
        /// 技能释放器
        /// </summary>
        /// <param name="info"></param>
        public void RunCast(CastInfo info)
        {
           var skill= Owner.SkillMgr.GetSkill(info.SkillId);
            if (skill.IsUnitTarget)
            {
                SpellTarget(info.SkillId,info.TargetId);
            }
            if (skill.IsPointTarget)
            {
                SpellPosition(info.SkillId, info.Targetloc);
            }
            if (skill.IsNoneTarget)
            {
                SpellNoTarget(info.SkillId);
            }
        }
        /// <summary>
        /// 释放无目标技能
        /// </summary>
        /// <param name="skill_id"></param>
        public void SpellNoTarget(int skill_id)
        {
            Log.Information("SpellNoTarget:{0},{1}",Owner.entityId,skill_id);
            //技能存在？
            var skill = Owner.SkillMgr.GetSkill(skill_id);
            if (skill == null)
            {
                Log.Warning("Spell::SpellTarget():角色:{0},技能:{1}Not found", Owner.entityId, skill_id);
                return;
            }
            //执行技能
            SCObject sco = new SCEntity(Owner);
            //技能可用？
            var res = skill.CanUse(sco);
            if (res != CastResult.Success)
            {
                //通知玩家释放技能失败
                OnSpellFailure(skill_id, res);
                return;
            }
            //使用技能
            skill.Use(sco);
            //技能信息
            CastInfo info = new CastInfo()
            {
                CasterId = Owner.entityId,
                SkillId = skill_id
            };
            Owner.Space.fightMgr.SpellQueue.Enqueue(info);
        }
        /// <summary>
        /// 释放单位目标技能
        /// </summary>
        public void SpellTarget(int skill_id,int target_id)
        {
            Log.Information("SpellTarget:玩家:{0},技能:{1}", Owner.entityId, skill_id);
            //技能存在？
            var skill = Owner.SkillMgr.GetSkill(skill_id);
            if (skill == null)
            {
                Log.Warning("Spell::SpellTarget():角色:{0},技能:{1}Not found", Owner.entityId, skill_id);
                return;
            }
            //目标存在?
           var target=Game.GetUnit(target_id);
            if (target==null)
            {
                Log.Warning("Spell::SpellTarget():目标对象:{0}Not found", target_id);
                return;
            }
            //执行技能
            SCObject sco = new SCEntity(target);
            //技能可用？
           var res= skill.CanUse(sco);
            if (res!=CastResult.Success)
            {
                //通知玩家释放技能失败
                OnSpellFailure(skill_id,res);
                return;
            }
            //使用技能
            skill.Use(sco);
            //
            CastInfo info = new CastInfo()
            {
                CasterId = Owner.entityId,
                TargetId = target_id,
                SkillId=skill_id
            };
            Owner.Space.fightMgr.SpellQueue.Enqueue(info);
        }
        /// <summary>
        /// 释放位置目标技能
        /// </summary>
        public void SpellPosition(int skill_id,Vector3 position)
        {
            Log.Information("SpellPosition:{0},{1}", Owner.entityId, skill_id);
            //执行技能
            SCObject sco = new SCPosition(position);
        }
        /// <summary>
        /// 通知玩家技能释放失败
        /// </summary>
        public void OnSpellFailure(int skill_id,CastResult reason)
        {
            if (Owner is Character chr)
            {
                SpellFailResponse resp = new SpellFailResponse()
                {
                    CasterId = chr.entityId,
                    SkillId = skill_id,
                    Reason = reason
                };
                chr.conn.Send(resp);
            }
        }
    }
}
