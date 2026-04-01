/// <summary>
/// 击杀怪物事件参数
/// </summary>
public class KillMonsterEventArgs
{
    public int monsterId; // 被击杀的怪物ID
    public int killCount; // 击杀数量（默认1）
    public KillMonsterEventArgs(int Id,int count)
    {
        this.monsterId = Id;
        this.killCount = count;
    }
}

/// <summary>
/// 收集道具事件参数
/// </summary>
public class CollectItemEventArgs
{
    public int itemId; // 收集的道具ID
    public int collectCount; // 收集数量
    public CollectItemEventArgs(int Id, int count)
    {
        this.itemId = Id;
        this.collectCount = count;
    }
}
/// <summary>
/// 等级提升事件参数
/// </summary>
public class LevelUpEventArgs
{
    public int newLevel; // 新等级
}
