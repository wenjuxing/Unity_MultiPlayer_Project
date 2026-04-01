- ScriptableObject数据：任务的基础数据在Unity的SO静态文件中编写，包含任务Id，任务内容，任务奖励，任务进度等；
---
- Json文件：存储任务数据，客户端服务端双端持有
```json
{

    "taskId": 1001,

    "taskType": 0,

    "taskState": 0,

    "taskName": "购买小型生命药水",

    "taskDesc": "小型生命药水是游戏中常见的基础恢复道具，瓶身小巧便携，澄澈的红色液体泛着微光。使用后可快速恢复玩家15%~20% 的最大生命值，冷却时间短，无使用门槛。 它适合战斗间隙应急回血，或探索时应对小额损伤，可通过商店购买、怪物掉落、宝箱开启获取，是新手前期冒险的必备之物",

    "canAbandon": true,

    "progress": {

      "progressType": 1,

      "targetParam": 1002,

      "targetValue": 3,

      "progressDesc": "进度:"

    },

    "rewards": {

      "rewardType": 1,

      "rewardId": 1002,

      "count": 10,

      "description": "小爷赏你的！"

    },

    "sortOrder": 200

  },
```
---
- Json文件映射类：通过类映射json文件任务字段，例如taskId,taskType,taskState等；
```csharp
// 任务配置主类（对应JSON数组中的单个任务对象）
public class TaskDefine
{
    [JsonProperty("taskId")]
    public int TaskId { get; set; }

    [JsonProperty("taskType")]
    public TaskType TaskType { get; set; }

    [JsonProperty("taskState")]
    public TaskState TaskState { get; set; }

    [JsonProperty("taskName")]
    public string TaskName { get; set; } = string.Empty;

    [JsonProperty("taskDesc")]
    public string TaskDesc { get; set; } = string.Empty;

    [JsonProperty("canAbandon")]
    public bool CanAbandon { get; set; }

    [JsonProperty("progress")]
    public TaskProgressDefine ProgressTargets { get; set; } = new TaskProgressDefine(); // 初始化空列表，避免null

    [JsonProperty("rewards")]
    public TaskRewardDefine Rewards { get; set; } = new TaskRewardDefine(); // 初始化空列表，避免null

    [JsonProperty("sortOrder")]
    public int SortOrder { get; set; } = 0; // 默认排序权重0
}
```
---
- DataManager(数据管理器)：加载任务json文件，把json的任务id作为键，把任务映射类作为值存储到字典当中，外部可以通过任务Id访问到任务具体数据；