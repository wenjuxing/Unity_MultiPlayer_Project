using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 对话章节表
/// </summary>
[Serializable]
public class DialogueMain
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("name")]
    public string name;

    [JsonProperty("firstGroupId")]
    public int firstGroupId;

    [JsonProperty("needSyncServer")]
    public bool needSyncServer = false;
}

/// <summary>
/// 对话组表
/// </summary>
[Serializable]
public class DialogueGroup
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("chapterId")]
    public int chapterId;

    [JsonProperty("nextGroupId")]
    public int nextGroupId;

    [JsonProperty("triggerEvent")]
    public string triggerEvent; // 格式："StartTask,1001"
}

/// <summary>
/// 对话数据
/// </summary>
[Serializable]
public class DialogueData
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("groupId")]
    public int groupId;

    [JsonProperty("characterName")] 
    public string characterName;

    [JsonProperty("content")]
    public string content;

    [JsonProperty("Position")] // 与JSON中的"Position"大小写保持一致
    public string Position; // 暂用string，后续解析为枚举

    [JsonProperty("characterIcon")] // 若Excel有此列，JSON会自动包含，无则忽略
    public string characterIcon;

    [JsonProperty("showTime")]
    public float showTime = 2f;

    [JsonProperty("characterId")]
    public int characterId;
}

/// <summary>
/// 选择选项表
/// </summary>
[Serializable]
public class DialogueOption
{
    [JsonProperty("id")]
    public int id;

    [JsonProperty("groupId")]
    public int groupId;

    [JsonProperty("content")]
    public string content;

    [JsonProperty("targetGroupId")]
    public int targetGroupId;

    [JsonProperty("triggerEvent")]
    public string triggerEvent;

    [JsonProperty("isVisible")]
    public bool isVisible = true;

    [JsonProperty("condition")]
    public string condition;
}