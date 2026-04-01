using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 任务策略工厂
/// </summary>
public class TaskStrategyFactory:SingletonBase<TaskStrategyFactory>
{
    //策略缓存避免重复创建
    private Dictionary<TaskProgressType, ITaskStrategy> _strategyCache = new Dictionary<TaskProgressType, ITaskStrategy>();
   
    private TaskStrategyFactory()
    {
        _strategyCache.Add(TaskProgressType.KillMonster,new KillMonsterTaskStrategy());
        _strategyCache.Add(TaskProgressType.CollectItem,new CollectItemTaskStrategy());
    }
    /// <summary>
    /// 根据任务类型获取策略实例
    /// </summary>
    /// <param name="taskType"></param>
    /// <returns></returns>
    public ITaskStrategy GetStrategy(TaskProgressType taskType)
    {
        if (_strategyCache.TryGetValue(taskType,out ITaskStrategy strategy))
        {
            return strategy;
        }
        throw new ArgumentException($"未找到任务类型{taskType}对应的策略");
    }
}
