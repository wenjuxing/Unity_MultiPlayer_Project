using GameServer.Database;
using GameServer.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using Google.Protobuf.Collections;
using Serilog;

namespace GameServer.TaskSystem
{
   public class TaskManager
    {
        public Character chr;
        public ConcurrentDictionary<int, TaskItem> TaskItems = new ConcurrentDictionary<int, TaskItem>();
        public TaskManager(Character chr)
        {
            this.chr = chr;
            this.Init();
        }
        /// <summary>
        /// 从数据库加载任务信息
        /// </summary>
        public void Init()
        {
            // 方法内创建仓储（推荐，避免多线程共享仓储实例）
            var repo = DataBase.fsql.GetRepository<PlayerTaskState>();

            //首次进入游戏
            //读取配置表信息到数据库中
            if(repo.Where(t => t.Id == chr.Id).Count() == 0)
            {
                foreach(var task in DataManager.Instance.Tasks)
                {
                    var taskState = new PlayerTaskState()
                    {
                        Id = chr.Id,
                        TaskId = task.Key,
                        IsAccepted = false,
                        IsCompleted = false,
                        IsSubmitted = false,
                        IsAbandoned = false,
                        CurrentProgress = 0, // 初始化进度：如2个目标则为"0,0"
                    };
                    repo.Insert(taskState);
                }
            }          
            //获取玩家任务列表
            List<PlayerTaskState> dbStates = repo.Where(t => t.Id == chr.Id).ToList();
            //把任务列表存入字典
            foreach (var state in dbStates)
            {
                //任务已经放弃则从字典中移除并跳出本次循环
                if (state.IsAbandoned)
                {
                    TaskItems.TryRemove(state.TaskId, out var task);
                    continue;
                }
                var item= new TaskItem(state.TaskId);
                if (state.IsAccepted)
                {
                    item.state = TaskState.InProgress;
                    item.NetTaskInfo.TaskState = (Proto.TaskState)TaskState.InProgress;
                }
                if (state.IsCompleted)
                {
                    item.state = TaskState.Completed;
                    item.NetTaskInfo.TaskState = (Proto.TaskState)TaskState.Completed;
                }
                if(state.IsSubmitted)
                {
                    item.state = TaskState.Finished;
                    item.NetTaskInfo.TaskState = (Proto.TaskState)TaskState.Finished;
                }
                item.NetTaskInfo.ProgressTargets.TargetValue = state.CurrentProgress;
                item.progressTargets.TargetValue = state.CurrentProgress;
                TaskItems[state.TaskId] = item;
            }
        }
        /// <summary>
        /// 标记数据中的任务为放弃状态
        /// </summary>
        /// <param name="taskId"></param>
        public async void AbandonTaskData(int taskId)
        {
            if (!DataManager.Instance.Tasks.ContainsKey(taskId)) return;
            var repo = DataBase.fsql.GetRepository<PlayerTaskState>();
            PlayerTaskState taskState = new PlayerTaskState()
            {
                Id = chr.Id,
                TaskId = taskId,
                IsAbandoned = true
            };
            await repo.UpdateAsync(taskState);
        }
        /// <summary>
        /// 更新数据到数据库
        /// </summary>
        public void UpdateTaskData(int taskId, bool? IsAccepted = null, bool? IsCompleted = null, bool? IsSubmitted = null, int? val = null)
        {
            // 方法内创建仓储（推荐，避免多线程共享仓储实例）
            var repo = DataBase.fsql.GetRepository<PlayerTaskState>();
            var update = repo.Where(t => t.TaskId == taskId && t.Id == chr.Id).First();
            if (update == null)
            {
                Log.Information("数据不存在{0}，{1}", taskId, chr.Id);
            }
            if (IsAccepted.HasValue) update.IsAccepted = (bool)IsAccepted;
            if (IsCompleted.HasValue) update.IsCompleted = (bool)IsCompleted;
            if (IsCompleted.HasValue) update.IsCompleted = (bool)IsCompleted;
            if (IsSubmitted.HasValue) update.IsSubmitted = (bool)IsSubmitted;
            if (val.HasValue) update.CurrentProgress = (int)val;
            repo.Update(update);
        }
    }
}
