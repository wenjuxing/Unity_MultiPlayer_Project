using Assets.Scripts.U3d_scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetGameManager : MonoBehaviour
{
    private void Start()
    {
    
    }

    private void Update()
    {
        if (GameApp.character == null) return;
        //模拟完成任务
        if (Input.GetKeyDown(KeyCode.Alpha1))
        TaskEventCenter.TriggerKillMonster(GameApp.character.entityId,new KillMonsterEventArgs(1001, 1));
        if (Input.GetKeyDown(KeyCode.Alpha2))
        TaskEventCenter.TriggerCollectItem(GameApp.character.entityId,new CollectItemEventArgs(1001, 1));
        if (Input.GetKeyDown(KeyCode.Alpha3))
        TaskEventCenter.TriggerCollectItem(GameApp.character.entityId,new CollectItemEventArgs(1002, 1));
    }
}
