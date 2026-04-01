using GameServer.Core;
using GameServer.FSM;
using GameServer.Mgr;
using GameServer.Model;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    public class MonsterAI : AIBase
    {
        public FsmSystem<Param> fsmSystem;
        public class Param
        {
            public Monster Owner;
            public int viewRange = 8000;
            public int walkRange = 8000;
            public int chaseRange = 15000;
            public int attackRange = 1500;
            public Random random = new Random();
        }
        public MonsterAI(Monster Owner):base(Owner)
        {
            Param param = new Param();
            param.Owner = Owner;
            fsmSystem = new FsmSystem<Param>(param);
            fsmSystem.AddState("walk",new WalkState());
            fsmSystem.AddState("chase",new ChaseState());
            fsmSystem.AddState("return", new ReturnState());
        }
        public override void Update()
        {
            fsmSystem?.Update();
        }
        /// <summary>
        /// 巡逻状态
        /// </summary>
        class WalkState : State<Param>
        {
            float lastTime = Time.time;
            float waitTime = 10f;
            public override void OnEnter()
            {
                Param.Owner.StopMove();
            }
            public override void OnUptate()
            {
                var mon = Param.Owner;

                //切换追击状态？
                var chr = Game.RangeUnit(mon.Space.Id, mon.Position, Param.viewRange)
                    .OfType<Character>()
                    .OrderBy(e => Vector3Int.Distance(mon.Position, e.Position))
                    .FirstOrDefault(c => !c.IsDeath);
                if (chr!=null)
                {
                    //设置追击目标
                    mon.target = chr;
                    //切换状态
                    FSM.ChangeState("chase");
                    return;
                }
                //巡逻
                if (mon.state==Proto.EntityState.Idle)
                {
                    if (lastTime + waitTime < Time.time)
                    {
                        lastTime = Time.time;
                        waitTime = (float)(Param.random.NextDouble()*20f) + 10f;
                        //移动到目标位置
                        var target = mon.RandomPointWithBirth(Param.walkRange);
                        mon.MoveTo(target);
                    }
                }
            }
        }
        /// <summary>
        /// 追击状态
        /// </summary>
        class ChaseState : State<Param>
        {
            public override void OnUptate()
            {
                //获取Monster
                var mon = Param.Owner;
                //目标存在或死亡?
                if (mon.target==null||mon.target.IsDeath||!EntityManager.Instance.Exit(mon.target.entityId))
                {
                    //清空目标
                    mon.target = null;
                    //切换状态
                    FSM.ChangeState("walk");
                    return;
                }
                //自身和出生地的距离
                float dict1 = Vector3.Distance(mon.Position,mon.initPositon);
                //自身和目标的距离
                float dict2 = Vector3.Distance(mon.Position,mon.target.Position);
                //切换返回状态？
                if (dict1>Param.chaseRange|| dict2>Param.viewRange)
                {
                    FSM.ChangeState("return");
                    return;
                }
                //切换攻击状态？
                if (dict2<=Param.attackRange)
                {
                    if (mon.state == Proto.EntityState.Move)
                        mon.StopMove();
                    mon.Attack(mon.target);
                }
                else
                {
                    mon.MoveTo(mon.target.Position);
                }
                
            }
        }
        /// <summary>
        /// 返回状态
        /// </summary>
        class ReturnState : State<Param>
        {
            public override void OnEnter()
            {
                //前往出生点
                Param.Owner.MoveTo(Param.Owner.initPositon);
            }
            public override void OnUptate()
            {
                var mon = Param.Owner;
                if (Vector3.Distance(mon.Position,mon.initPositon)<100)
                {
                    FSM.ChangeState("walk");
                    return;
                }
            }
        }
    }
}
