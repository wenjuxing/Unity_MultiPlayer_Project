using GameServer.Core;
using GameServer.Fight;
using GameServer.Mgr;
using GameServer.Model;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
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
        public bool IsPassive;       //是否被动技能 
        public SkillDefine Define;   //技能设定
        public Actor Owner;          //技能所属者
        public float Cooldown;       //冷却计时
        private float _time;        //技能运行时间
        public SkillState State;     //当前技能状态 
        //强制暴击
        int forceCritAfter => (int)(100f / Owner.Attr.Fianl.CRI+10e-6) + 2;
        //未暴击次数
         int notCrit = 0;
        public SCObject Target { get; private set; }
        public bool IsUnitTarget { get => Define.TargetType == "单位"; }
        public bool IsPointTarget { get => Define.TargetType == "点"; }
        public bool IsNoneTarget { get => Define.TargetType == "None"; }
        public bool IsNormal => Define.Type == "普通攻击";
        public FightMgr FightMgr => Owner.Space.fightMgr;
        public Skill(Actor owner,int skillId)
        {
            this.Owner = owner;
            Define = DataManager.Instance.Skills[skillId];
            //伤害延迟时间数组长度=0？
            if (Define.HitDelay.Length==0)
            {
                Array.Resize(ref Define.HitDelay,1);
            }
        }
        public void Update()
        {
            if (State == SkillState.None && Cooldown == 0) return;

            //计算冷却时间
            if (Cooldown > 0) Cooldown -= Time.deltaTime;
            if (Cooldown < 0) Cooldown =0;

            //技能运行计时
            _time += Time.deltaTime;

            //蓄力状态=>激活状态
            if (State==SkillState.Casting&&_time>=Define.IntonateTime)
            {
                State = SkillState.Active;
                Cooldown = Define.CD;
                OnActive();
            }
            //激活状态=>冷却状态
            if (State==SkillState.Active)
            {
                if (_time>=Define.IntonateTime+Define.HitDelay.Max())
                {
                    State = SkillState.Coolding;
                }
            }
            //冷却状态=>无状态
            if (State==SkillState.Coolding)
            {
                if (Cooldown == 0)
                {
                    _time = 0;
                    State = SkillState.None;
                    OnFinish();
                }
            }
        }
        /// <summary>
        /// 技能激活时调用
        /// </summary>
        private void OnActive()
        {
            Log.Information("技能激活：Skill[{0}],Owner[{1}]", Define.Name, Owner.entityId);
            //技能为投射物？
            if (Define.IsMissile)
            {
                //创建实例
                Missile missile = new Missile(this,Owner.Position, Target);
                //加入战斗管理器的投射物列表
                FightMgr.Missiles.Add(missile);
            }
            else
            {
                Log.Information("Def.HitDelay.Length=" + Define.HitDelay.Length);
                for (int i = 0; i < Define.HitDelay.Length; i++)
                {
                    Scheduler.Instance.AddTask(_hitTrigger, Define.HitDelay[i], 1);
                }
            }
        }
        //触发延迟伤害
        private void _hitTrigger()
        {
            Log.Information("_hitTrigger:Owner[{0}],Skill[{1}]", Owner.entityId,Define.Name);
            OnHit(Target);
        }
        public void OnHit(SCObject sco)
        {
            Log.Information("OnHit:Owner[{0}],Skill[{1}],SCO[{2}]", Owner.entityId,Define.Name,sco);
            //单体伤害
            if (Define.Area==0)
            {
                if (sco is SCEntity sCEntity)
                {
                    var actor = sCEntity.RealObj as Actor;
                    TakeDamaged(actor);
                }
            }
            //范围伤害 发起进攻
            else
            {
                //获取伤害范围内的角色列表
                var list = Game.RangeUnit(Owner.Space.Id,sco.Position,Define.Area);
                //对每个角色造成伤害
                list.ForEach(item=>TakeDamaged(item));
            }
        }
        Random random = new Random();
        private void TakeDamaged(Actor target)
        {
            //是攻击目标？
            if (target.IsDeath || target == Owner||target==null) return;
            Log.Information("TakeDamaged:Owner[{0}],Target[{1}]",Owner.entityId,target.entityId);
            //伤害公式=攻击值*（1-防御值/（防御值+400+85*等级[攻]））
            //获取双方的属性
            var a = Owner.Attr.Fianl;
            var b = target.Attr.Fianl;
            //伤害信息
            Damage damage = new Damage();
            damage.AttackerId = Owner.entityId;
            damage.TargetId = target.entityId;
            damage.SkillId = Define.ID;
            //计算攻击方的物攻和法攻
            var ad = Define.AD + a.AD * Define.ADC;
            var ap = Define.AP + a.AP * Define.APC;
            //new
            if (b == null) return;
            //计算伤害
            var ads = ad * (1 - b.DEF / (b.DEF + 400 + 85 * Owner.info.Level));
            var aps = ap * (1 - b.MDEF / (b.MDEF + 400 + 85 * Owner.info.Level));
            Log.Information("ADS[{0}],APS[{1}]",ads,aps);
            //计算伤害总量
            damage.Amount = ads + aps;
            //计算暴击
            notCrit++;
            float randCri = (float)random.NextDouble();
            float cri = a.CRI * 0.01f;
            Log.Information("暴击计算:{0}/{1},|[{2}/{3}]",randCri,cri,notCrit,forceCritAfter);
            //满足随机暴击率<理论暴击率||没有暴击次数>强制暴击次数
            if (randCri < cri||notCrit>forceCritAfter)
            {
                notCrit = 0;
                damage.IsCrit = true;
                damage.Amount *= Math.Max(a.CRD,100) *0.01f;
            }
            //计算闪避
            //受击率=（命中率[攻]-闪避率[防]）
            Log.Information("a.HitRate[{0}],b.DodgeRate[{1}]", a.HitRate ,b.DodgeRate);
            var hitRate = (a.HitRate - b.DodgeRate) * 0.01f;
            Log.Information("hitRate:{0}",hitRate);
            if (random.NextDouble()>hitRate)
            {
                damage.IsMiss = true;
                damage.Amount = 0;
            }
            //造成伤害
            target.RecvDamage(damage);
            Log.Information("Amount:[{0}]", damage.Amount);
        }

        /// <summary>
        /// 技能结束
        /// </summary>
        private void OnFinish()
        {
            Log.Information("技能结束：Skill[{0}],Owner[{1}]", Define.Name, Owner.entityId);
        }
        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        /// <returns></returns>
        public CastResult CanUse(SCObject sco)
        {
            //被动技能
            if (IsPassive) return CastResult.IsPassive;
            //MP不足
            else if (Define.Cost > Owner.info.Mp) return CastResult.MpLack;
            //正在释放其他技能
            else if (State != SkillState.None) return CastResult.Running;
            //技能冷却中
            else if (Cooldown > 0) return CastResult.Cooldown;
            //施法者死亡
            else if (Owner.IsDeath) return CastResult.EntityDead;
            //目标死亡
            else if (sco is SCEntity && (sco.RealObj as Actor).IsDeath) return CastResult.EntityDead;
            //施法者和目标的距离
            //todo 获取目标位置
            var dict = Vector3Int.Distance(Owner.Position,sco.Position);
            if (dict > Define.SpellRange) return CastResult.OutOfRange;

            //返回成功
            return CastResult.Success;
        } 
        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="sco"></param>
        /// <returns></returns>
        public CastResult Use(SCObject sco)
        {
            Target = sco;
            _time = 0;
            //进入技能蓄力状态
            State = SkillState.Casting; 
            return CastResult.Success;
        }
    }
}
