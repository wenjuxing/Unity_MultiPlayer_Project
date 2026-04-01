using Assets.Scripts.U3d_scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
   
    void Start()
    {
        //dropdown.onValueChanged.AddListener(SetScreenSize);
    }
    private void SetScreenSize(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1280,720,false);
                break;
            case 1:
                Screen.SetResolution(800, 600, false);
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }
}
