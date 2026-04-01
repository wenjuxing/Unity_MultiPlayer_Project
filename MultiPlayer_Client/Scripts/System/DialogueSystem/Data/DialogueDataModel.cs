using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDataModel
{
    public static Dictionary<int, List<DialogueData>> dialogueDataMap = new Dictionary<int, List<DialogueData>>();
    public static Dictionary<int, List<DialogueOption>> dialogueOptionMap = new Dictionary<int, List<DialogueOption>>();
    public static void InitData()
    {
        //对话数据
        foreach (var data in DataManager.Instance.dialogueDatas)
        {
            if (!dialogueDataMap.ContainsKey(data.Value.groupId))
            {
                var dialogueDataList = new List<DialogueData>();
                dialogueDataMap.Add(data.Value.groupId, dialogueDataList);
            }
            dialogueDataMap[data.Value.groupId].Add(data.Value);
        }
        //对话选项
        foreach (var data in DataManager.Instance.dialogueOptions)
        {
            if (!dialogueOptionMap.ContainsKey(data.Value.groupId))
            {
                var dialogueOptionsList = new List<DialogueOption>();
                dialogueOptionMap.Add(data.Value.groupId, dialogueOptionsList);
            }
            dialogueOptionMap[data.Value.groupId].Add(data.Value);
        }
    }
}
