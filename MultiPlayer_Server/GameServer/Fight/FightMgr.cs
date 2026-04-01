using GameServer.Core;
using GameServer.Mgr;
using GameServer.Model;
using Proto;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameServer.Fight
{
    public class FightMgr
    {
        /// <summary>
        /// 战斗管理器
        /// </summary>
        private Space Space { get; }
        //场景中的飞行物
        public List<Missile> Missiles = new List<Missile>();
        //技能施法队列
        public ConcurrentQueue<CastInfo> CastQueue = new ConcurrentQueue<CastInfo>();
        //等待广播队列
        public ConcurrentQueue<CastInfo> SpellQueue = new ConcurrentQueue<CastInfo>();
        //等待广播伤害队列
        public ConcurrentQueue<Damage> DamageQueue = new ConcurrentQueue<Damage>();
        //角色属性变化
        public ConcurrentQueue<PropertyUpdate> PropertyUpdateQueue = new ConcurrentQueue<PropertyUpdate>();
        //施法响应对象，每帧执行一次
        private SpellResponse SpellResponse=new SpellResponse(); 
        //伤害响应对象
        private DamageResponse DamageResponse = new DamageResponse();
        //属性变化响应对象
        private PropertyUpdateResponse PropertyUpdateResponse = new PropertyUpdateResponse();
        public FightMgr(Space space)
        {
            this.Space = space;
        }
        public void OnUpdate(float detla)
        {
            while (CastQueue.TryDequeue(out CastInfo cast))
            {
                Log.Information($"执行施法{cast}");
                RunCast(cast);
            }
            //更新飞行物状态
            foreach (var item in Missiles)
            {
                item.OnUpdate(detla);
            }
            BroadCastSpell();
            BroadCastDamage();
            BroadCastProperties();
        }
        /// <summary>
        /// 广播伤害
        /// </summary>
        private void BroadCastDamage()
        {
            while(DamageQueue.TryDequeue(out Damage item))
            {
                DamageResponse.List.Add(item);
            }
            if (DamageResponse.List.Count>0)
            {
                Space.BroadCast(DamageResponse);
                DamageResponse.List.Clear();
            }
        }
        /// <summary>
        /// 广播新的属性
        /// </summary>
        private void BroadCastProperties()
        {
            while (PropertyUpdateQueue.TryDequeue(out var item))
            {
                PropertyUpdateResponse.List.Add(item);
            }
            if (PropertyUpdateResponse.List.Count>0)
            {
                Space.BroadCast(PropertyUpdateResponse);
                PropertyUpdateResponse.List.Clear();
            }
        }
        /// <summary>
        /// 广播施法信息
        /// </summary>
        private void BroadCastSpell()
        {
            while (SpellQueue.TryDequeue(out CastInfo item))
            {
                SpellResponse.CastList.Add(item);
            }
            //使用地图广播功能广播消息
            Space.BroadCast(SpellResponse);
            //清空上次加入临时列表的技能
            SpellResponse.CastList.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cast"></param>
        private void RunCast(CastInfo cast)
        {
            //获取施法者
           var caster= Game.GetUnit(cast.CasterId); 
            //施法者为空？
            if (caster==null)
            {
                Log.Error($"RunCast:caster is null{cast.CasterId}");
                return;
            }
            //调用技能释放器
            caster.Spell.RunCast(cast);
        }
    }
}
