using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.FSM
{
   public class FsmSystem<T>
    {
        private Dictionary<string, State<T>> _dict = new Dictionary<string, State<T>>();
        public State<T> CurrentState { get; private set; }
        public string CurrentStateId { get; private set; }
        //共享参数
        public T param;

        public FsmSystem(T param)
        {
            this.param = param;
        }
        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="state"></param>
        public void AddState(string Id,State<T> state)
        {
            if (CurrentStateId==null)
            {
                CurrentStateId = Id;
                CurrentState = state;
            }
            _dict[Id] = state;
            state.FSM = this;
        }
        /// <summary>
        /// 移除状态
        /// </summary>
        /// <param name="Id"></param>
        public void RemoveState(string Id)
        {
            if (_dict.ContainsKey(Id)) 
            {
                _dict[Id].FSM = null;
                _dict.Remove(Id);
            }
        }
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="Id"></param>
        public void ChangeState(string Id)
        {
            if (CurrentStateId == Id) return;
            if (!_dict.ContainsKey(Id)) return;
            if (CurrentState != null) CurrentState.OnExit();
            CurrentStateId = Id;
            CurrentState = _dict[Id];
            CurrentState.OnEnter();
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            CurrentState?.OnUptate();
        }
    }
}
