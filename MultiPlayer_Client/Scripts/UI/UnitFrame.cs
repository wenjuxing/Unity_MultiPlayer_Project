using Assets.Scripts.U3d_scripts;
using GameClient.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitFrame : MonoBehaviour
{
    [SerializeField] Image HealthBar;
    [SerializeField] Image ManaBar;
    [SerializeField] Text Level;
    private Text Name;
    private CanvasGroup canvasGroup;
    public Actor actor;
    private void Start()
    {
        Name = transform.Find("Name").GetComponent<Text>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
    private void Update()
    {
        if (actor != null)
        {
            canvasGroup.alpha = 1;
            Name.text = actor.Info.Name;
            Level.text = actor.Info.Level.ToString();
            HealthBar.fillAmount = actor.Info.Hp / actor.define.HPMax;
            ManaBar.fillAmount = actor.Info.Mp / actor.define.MPMax;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
}
