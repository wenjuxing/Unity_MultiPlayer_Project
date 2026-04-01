using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
   public class AttributesAssembly
    {
        private Attributes Basic; //基础属性(基础+成长)
        private Attributes Equip; //装备属性
        private Attributes Buffs; //Buff属性
        public Attributes Fianl; //Fianl属性
        public void Init(Actor actor)
        {
            Basic = new Attributes();
            Equip = new Attributes();
            Buffs = new Attributes();
            Fianl = new Attributes();

            var define = actor.Define;
            var Level = actor.info.Level;

            //初始属性
            var Initial = new Attributes();
            Initial.Speed = define.Speed;
            Initial.HPMax = define.HPMax;
            Initial.MPMax = define.MPMax;
            Initial.AD = define.AD;
            Initial.AP = define.AP;
            Initial.DEF = define.DEF;
            Initial.MDEF = define.MDEF;
            Initial.CRI = define.CRI;
            Initial.CRD = define.CRD;
            Initial.STR = define.STR;
            Initial.INT = define.INT;
            Initial.AGI = define.AGI;
            Initial.HitRate = define.HitRate;
            Initial.DodgeRate = define.DodgeRate;
            Initial.HpRegen = define.HpRegen;
            Initial.HpSteal = define.HpSteal;
            //成长属性
            var Growth = new Attributes();
            Growth.STR = define.STR * Level;
            Growth.INT = define.INT * Level;
            Growth.AGI = define.AGI * Level;
            //基础属性(初始+成长)
            Basic.Add(Initial);
            Basic.Add(Growth);

            //todo 装备和Buffs

            //最终属性
            Fianl.Add(Basic);
            Fianl.Add(Equip);
            Fianl.Add(Buffs);
            //计算附加属性
            var Extra = new Attributes();
            Extra.HPMax = Fianl.STR * 5;
            Extra.MPMax = Fianl.INT * 1.5f;
            Fianl.Add(Extra);
            
            //Log.Information("初始属性:{0}",Initial);
            //Log.Information("基础属性:{0}", Basic);
            //Log.Information("装备属性:{0}", Equip);
            //Log.Information("Buffs属性:{0}", Buffs);
            //Log.Information("最终属性:{0}", Fianl);
        }
    }
}
