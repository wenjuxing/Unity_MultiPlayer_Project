using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proto;
using Serilog;

namespace GameServer.TaskSystem
{
    /// <summary>
    /// 任务条基类
    /// </summary>
    [Serializable]
   public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool canAbandon { get; set; }
        public TaskType type { get; set; }
        public TaskState state { get; set; }
        public TaskProgressDefine progressTargets;
        public TaskRewardDefine taskRewards;
        public TaskDefine taskDefine { get; }
        private NetTaskInfo _taskInfo;
        public NetTaskInfo NetTaskInfo 
        {
            get 
            {
                if (_taskInfo==null)
                {
                    _taskInfo = new NetTaskInfo()
                    {
                        TaskId = Id,
                        TaskType = (Proto.TaskType)type,
                        TaskDesc = Description,
                        TaskName = Name,
                        CanAbandon = canAbandon,
                        TaskState = (Proto.TaskState)state,
                        ProgressTargets = new TaskProgress()
                        {
                            ProgressType = (Proto.TaskProgressType)progressTargets.ProgressType,
                            TargetParam = progressTargets.TargetParam,
                            TargetValue = progressTargets.TargetValue
                        },
                        Rewards = new TaskReward()
                        {
                            RewardType = (Proto.RewardType)taskRewards.RewardType,
                            RewardId = taskRewards.RewardId,
                            Count = taskRewards.Count
                        }
                    };
                }
                return _taskInfo;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskItem(int Id,string Name,string Desc,bool canAbandon,TaskType type,TaskState state,
        TaskProgressDefine progressTargets, TaskRewardDefine taskRewards)
        {
            this.Id = Id;
            this.Name = Name;
            this.Description = Desc;
            this.canAbandon = canAbandon;
            this.type = type;
            this.state = state;
            this.progressTargets = progressTargets;
            this.taskRewards = taskRewards;
        }
        public TaskItem(int ItemId):this(DataManager.Instance.Tasks[ItemId])
        {

        }
        public TaskItem(TaskDefine def):this(def.TaskId,def.TaskName,def.TaskDesc,def.CanAbandon,def.TaskType,def.TaskState,def.ProgressTargets,def.Rewards)
        {
            this.taskDefine = def;
            Log.Information("操作taskDefine{0},{1}", this.taskDefine.TaskName,this.taskDefine.ProgressTargets.TargetValue);
        }
    }
}
