using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    bool IsOpen;
    private void Start()
    {
        BagMgr.Instance.InitItemsInfo();
        UIManager.Instance.ShowUI<BagPanel>();
    }
    public void OpenBag()
    {
        if (!IsOpen)
            UIManager.Instance.ShowUI<BagPanel>();
        else
            UIManager.Instance.HideUI("BagPanel");
        IsOpen = !IsOpen;
    }
  
}
