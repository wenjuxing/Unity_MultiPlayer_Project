using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using GameServer.AI;

namespace GameServer.Model
{
    public class Monster : Actor
    {
        public AIBase AI;
        public Actor target;

        public Vector3 moveTarget;    //目标位置
        public Vector3 movePosition;  //当前移动位置
        public Vector3 initPositon;
        private Random rand=new Random();
        private static Vector3Int Y1000= new Vector3Int(0, 1000, 0);
        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir)
            : base(EntityType.Monster, tid,level, pos, dir)
        {
            state = EntityState.Idle;
             this.initPositon = pos;
            //Random random = new Random();
            //广播消息
            Scheduler.Instance.AddTask(() => {
                if (state != EntityState.Move) return;
                NEntitySync res = new NEntitySync();
                res.Entity = EntityData;
                res.State = state;
                this.Space.UpdateEntity(res);
            }, 0.15f);
            //配置MonsterAI
            switch (Define.AI)
            {
                case "Monster":
                    this.AI = new MonsterAI(this);
                    break;
            }
        }
        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            state = EntityState.Idle;
            movePosition = moveTarget;
            NEntitySync res = new NEntitySync();
            res.Entity = EntityData;
            res.State = state;
            this.Space.UpdateEntity(res);
        }
        /// <summary>
        /// 设置移动目标点
        /// </summary>
        /// <param name="Vector3Int"></param>
        public void MoveTo(Vector3 target)
        {
            //如果处于闲置状态则修改为移动状态
            if (state==EntityState.Idle)
            {
                state = EntityState.Move;
            }
            //如果当前目标位置不等于目标位置
            if (moveTarget!=target)
            {
                //设置目标位置和当前位置
                moveTarget = target;
                movePosition = Position;
                //计算移动方向
                var dir= (moveTarget - movePosition).normalized;
                Direction = LookRotation(dir) * Y1000;
                //广播消息
                NEntitySync res = new NEntitySync();
                res.Entity = EntityData;
                res.State = state;
                this.Space.UpdateEntity(res);
            }
        }
        public override void Update()
        {
            base.Update();
            AI?.Update();
            if (state==EntityState.Move)
            {
                //计算客户端朝向
                var dir = (moveTarget - movePosition).normalized;
                this.Direction = LookRotation(dir) * Y1000;
                //计算移动的距离
                float dict = Speed * Time.deltaTime;
                if (Vector3.Distance(moveTarget,movePosition)<dict)
                {
                    StopMove();
                }
                else
                {
                    movePosition += dict * dir;
                }
                this.Position = movePosition;
            }
        }
        //方向向量转欧拉角
        public Vector3 LookRotation(Vector3 fromDir)
        {
            float Rad2Deg = 57.29578f;
            Vector3 eulerAngles = new Vector3();
            // 计算 AngleX
            double xzSquared = fromDir.x * fromDir.x + fromDir.z * fromDir.z;
            double magnitudeSquared = fromDir.x * fromDir.x + fromDir.y * fromDir.y + fromDir.z * fromDir.z;
            eulerAngles.x = (float)Math.Acos(Math.Sqrt(xzSquared / magnitudeSquared)) * Rad2Deg;
            if (fromDir.y > 0) eulerAngles.x = 360 - eulerAngles.x;
            // 计算 AngleY
            eulerAngles.y = (float)Math.Atan2(fromDir.x, fromDir.z) * Rad2Deg;
            if (eulerAngles.y < 0) eulerAngles.y += 180;
            if (fromDir.x < 0) eulerAngles.y += 180;
            // AngleZ 设为 0
            eulerAngles.z = 0;
            return eulerAngles;
        }
        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="target"></param>
        public void Attack(Actor target)
        {
            var skill = SkillMgr.Skills.FirstOrDefault(s=>s.IsNormal);
            if (skill == null) return;
            if (skill.State != Battle.SkillState.None) return;
            this.Spell.SpellTarget(skill.Define.ID,target.entityId);
        }

        public Vector3 RandomPointWithBirth(float range)
        {
            float x = (float)(rand.NextDouble() * 2f - 1f);
            float z = (float)(rand.NextDouble() * 2f - 1f);
            Vector3 dir = new Vector3(x, 0, z).normalized;
            return initPositon + dir * range * (float)rand.NextDouble();
        }
    }
}
