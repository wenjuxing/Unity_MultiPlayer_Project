using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    /// <summary>
    /// AI基础类
    /// </summary>
   public abstract class AIBase
    {
        public Monster Owner;
        public AIBase(Monster owner)
        {
            this.Owner = owner;
        }
        public abstract void Update();
    }
}
