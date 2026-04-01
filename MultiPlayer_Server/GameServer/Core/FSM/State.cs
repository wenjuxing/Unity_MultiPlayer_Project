using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.FSM
{
    /// <summary>
    /// 状态基础类
    /// </summary>
   public class State<T>
    {
        public FsmSystem<T> FSM;
        public T Param => FSM.param;
        public virtual void OnEnter() { }
        public virtual void OnUptate() { }
        public virtual void OnExit() { }
    }
}
