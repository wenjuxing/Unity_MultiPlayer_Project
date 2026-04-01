using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TaskConfig_", menuName = "游戏配置/任务配置")]
public class TaskConfig : ScriptableObject
{
    public TaskConfigData Data;
}