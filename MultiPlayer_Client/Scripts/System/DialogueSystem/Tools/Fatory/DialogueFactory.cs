using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对话工厂 数据加载
/// </summary>
public class DialogueFactory : IDialogueFactory
{
    public DialogueMain GetChapter(int chapterId)
    {
        return DataManager.Instance.dialogueMains[chapterId];
    }

    public DialogueGroup GetGroup(int groupId)
    {
        return DataManager.Instance.dialogueGroups[groupId];
    }
    /// <summary>
    /// 获取该组所有对话数据
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public List<DialogueData> GetGroupDatas(int groupId)
    {
        if (DialogueDataModel.dialogueDataMap.TryGetValue(groupId, out var dialogueDatas))
            return dialogueDatas;
        else return null;
    }
}
