using GameServer.Battle;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Fight
{
   public class Missile
    {
        //所属场景
        public Space Space { get;private set; }
        //所属技能
        public Skill skill { get; private set; }
        //释放目标
        public SCObject Target { get; private set; }
        //初始位置
        public Vector3 InitPos { get; private set; }
        //飞行物当前位置
        public Vector3 Position;
        public FightMgr FightMgr => Space.fightMgr;
        public Missile(Skill skill,Vector3 InitPos,SCObject target)
        {
            this.Space = skill.Owner.Space;
            this.skill = skill;
            this.InitPos = InitPos;
            this.Target = target;
            this.Position = InitPos;
        }

        public void OnUpdate(float dt)
        {
            var a = this.Position;
            var b = this. Target.Position;
            Vector3 dir = (b - a).normalized;
            float dict = skill.Define.MissileSpeed *dt;
            if (dict >= Vector3.Distance(b, a))
            {
                this.Position = b;
                skill.OnHit(Target);
                FightMgr.Missiles.Remove(this);
            }
            else
            {
                this.Position =this.Position + dict * dir;
            }
        }
    }
}
