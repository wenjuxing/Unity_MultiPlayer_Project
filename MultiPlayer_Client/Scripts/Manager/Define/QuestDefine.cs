using System.ComponentModel;

public enum QuestType
{
	[Description("主线")]
	Main,
	[Description("直线")]
	Branch
}
public enum QuestTarget
{
	None,
	Kill,
	Item
}
public class QuestDefine
{
	public int Key; // Key
	public int ID; // ID
	public string Name; // Name
	public int LimitLevel; // LimitLevel
	public int LimitClass; // LimitClass
	public int PreQuest; // PreQuest
	public int PostQuest; // PostQuest
	public QuestType Type { get; set; }
	public int AcceptNPC; // AcceptNPC
	public int SubmitNPC; // SubmitNPC
	public string Overview; // Overview
	public string Dialog; // Dialog
	public string DialogAccept; // DialogAccept
	public string DialogDeny; // DialogDeny
	public string DialogIncomplete; // DialogIncomplete
	public string DialogFinish; // DialogFinish
	public QuestTarget Target1; // Target1
	public int Target1ID; // Target1ID
	public int Target1Num; // Target1Num
	public QuestTarget Target2; // Target2
	public int Target2ID; // Target2ID
	public int Target2Num; // Target2Num
	public QuestTarget Target3; // Target3
	public int Target3ID; // Target3ID
	public int Target3Num; // Target3Num
	public int RewardGold; // RewardGold
	public int RewardExp; // RewardExp
	public int RewardItem1; // RewardItem1
	public int RewardItem1Count; // RewardItem1Count
	public int RewardItem2; // RewardItem2
	public int RewardItem2Count; // RewardItem2Count
	public int RewardItem3; // RewardItem3
	public int RewardItem3Count; // RewardItem3Count
}

