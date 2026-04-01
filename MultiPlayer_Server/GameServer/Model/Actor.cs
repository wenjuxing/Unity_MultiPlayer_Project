using Common.Core;
using GameServer.Battle;
using GameServer.Fight;
using GameServer.InventorySystem;
using GameServer.Mgr;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    public class Actor : Entity
    {
        public int Id { get { return info.Id; } set { info.Id = value; } }
        public string Name { get; set; }
        public Space Space { get; set; }
        //单位状态
        public UnitState UnitState;
        public bool IsDeath => UnitState == UnitState.Dead;
        public NetActor info { get; set; } = new NetActor();
        public EntityType Type { get {return info.EntityType; } set {info.EntityType=value; } }
        public UnitDefine Define { get; set; }
        public AttributesAssembly Attr { get; set; } = new AttributesAssembly();
        public SkillManager SkillMgr;
        public Spell Spell;

        public float Hp => info.Hp;
        public float Mp => info.Mp;
        public float HPMax => Attr.Fianl.HPMax;
        public float MPMax => Attr.Fianl.MPMax;
        public long Gold => info.Gold;
        public long Exp => info.Exp;
        public int Level => info.Level;
        public Actor(EntityType Type,int tid,int level,Vector3Int position, Vector3Int direction)
            : base(position, direction)
        {
            this.info.Level = level;
            this.info.Tid = tid;
            this.info.EntityType = Type;
            this.info.Entity = EntityData;
            if (Type != EntityType.Item)
            {
                this.Define = DataManager.Instance.Units[tid];
                this.info.Hp = (int)Define.HPMax;
                this.info.Mp = (int)Define.MPMax;
                this.Speed = Define.Speed;
                this.SkillMgr = new SkillManager(this);
                this.Spell = new Spell(this);
                this.Attr.Init(this);
                this.info.Name = Define.Name;
            }

        }
        /// <summary>
        /// 进入地图时设置所属地图以及地图Id
        /// </summary>
        /// <param name="space"></param>
        public void OnEnterSpace(Space space)
        {
            //当前场景和目标场景都存在
            if (Space!=null&&space!=null)
            {
                EntityManager.Instance.ChangeSpace(this,Space.Id,space.Id);
            }
            this.Space = space;
            this.info.SpaceId = space.Id;
            //设置数据库中的地图Id
            if(this is Character chr)
            {
                chr.Data.SpaceId = space.Id;
            }
        }
        /// <summary>
        /// 场景传送
        /// </summary>
        public virtual void TelePortSpace(Space space,Vector3Int Pos,Vector3Int Dir=new Vector3Int())
        {
            //传送者为角色？
            Character chr;
            if (this is Character) chr = (Character)this;
            else return;
            //跨场景传送
            if (space != Space)
            {
                //离开当前场景
                Space.EntityLeave(chr);
                //设置坐标和方向
                chr.Position = Pos;
                chr.Direction = Dir;
                //加入新的场景
                space.EntityEnter(chr);
            }
            //同场景传送
            else
            {
                space.TelePort(chr,Pos,Dir);
            }
        }
        /// <summary>
        /// 复活
        /// </summary>
        public void Revive()
        {
            SetHP(Attr.Fianl.HPMax);
            SetMP(Attr.Fianl.MPMax);
            SetState(UnitState.Free);
        }
        /// <summary>
        /// 帧函数
        /// </summary>
        public override void Update()
        {
            SkillMgr?.Update();
        }
        /// <summary>
        /// 接受伤害
        /// </summary>
        /// <param name="damage"></param>
        public void RecvDamage(Damage damage)
        {
            Log.Information("Acotr:RecvDamage[{0}]",damage);
            //添加广播
            Space.fightMgr.DamageQueue.Enqueue(damage);
            if (info.Hp > damage.Amount)
            {
                //设置血量
                SetHP(info.Hp - damage.Amount);
            }
            else
            {
                //死亡
                Die(damage.AttackerId);
            }
        }
        /// <summary>
        /// 设置血量
        /// </summary>
        public void SetHP(float hp)
        {
            //血量发生变化?
            if (MathC.Equals(info.Hp, hp)) return;
            if (hp <= 0)
            {
                hp = 0;
                //IsDeath = true;
                this.UnitState = UnitState.Dead;
            }
            if (hp>Attr.Fianl.HPMax)
            {
                hp = Attr.Fianl.HPMax;
            }
            float oldValue = info.Hp;
            info.Hp = hp;
            Log.Information("SetHP:Info.Hp[{0}],oldValue[{1}]", info.Hp,oldValue);
            //通知客户端属性更新
            PropertyUpdate propertyUpdate = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.Hp,
                OldValue = new PropertyUpdate.Types.PropertyValue() { FloatValue=oldValue},
                NewValue=new PropertyUpdate.Types.PropertyValue(){FloatValue=info.Hp }
            };
            //广播属性更新
            Space.fightMgr.PropertyUpdateQueue.Enqueue(propertyUpdate);
        }
        /// <summary>
        /// 设置蓝量
        /// </summary>
        /// <param name="hp"></param>
        public void SetMP(float mp)
        {
            //蓝量发生变化?
            if (MathC.Equals(info.Mp, mp)) return;
            if (mp <= 0)
            {
                mp = 0;
            }
            if (mp > Attr.Fianl.MPMax)
            {
                mp = Attr.Fianl.MPMax;
            }
            float oldValue = info.Mp;
            info.Mp = mp;
            Log.Information("SetMP:Info.Mp[{0}],oldValue[{1}]", info.Mp, oldValue);
            //通知客户端属性更新
            PropertyUpdate propertyUpdate = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.Mp,
                OldValue = new PropertyUpdate.Types.PropertyValue() { FloatValue = oldValue },
                NewValue = new PropertyUpdate.Types.PropertyValue() { FloatValue = info.Mp }
            };
            //广播属性更新
            Space.fightMgr.PropertyUpdateQueue.Enqueue(propertyUpdate);
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        private void SetState(UnitState unitState)
        {
            //状态改变？
            if (this.UnitState == unitState) return;
            //记录旧值
            UnitState oldValue = this.UnitState;
            this.UnitState = unitState;
            PropertyUpdate po = new PropertyUpdate()
            {
                EntityId = entityId,
                Property = PropertyUpdate.Types.Prop.State,
                OldValue = new PropertyUpdate.Types.PropertyValue { StateValue=oldValue},
                NewValue = new PropertyUpdate.Types.PropertyValue { StateValue=unitState}
            };
            Space.fightMgr.PropertyUpdateQueue.Enqueue(po);
        }
        /// <summary>
        /// 死亡
        /// </summary>
        public virtual void Die(int killerId)
        {
            if (IsDeath) return;
            BeforeDie(killerId);
            SetState(UnitState.Dead);
            SetHP(0);
            SetMP(0);
            AfterDie(killerId);
        }
        /// <summary>
        /// 死亡之前执行
        /// </summary>
        /// <param name="killerId"></param>
        protected virtual void BeforeDie(int killerId)
        {

        }
        /// <summary>
        /// 死亡之后执行
        /// </summary>
        /// <param name="killerId"></param>
        protected virtual void AfterDie(int killerId)
        {

        }
    }
}
