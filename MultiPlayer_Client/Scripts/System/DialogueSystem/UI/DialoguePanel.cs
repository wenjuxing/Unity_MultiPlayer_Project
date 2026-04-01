using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DialoguePanel : UIBase
{
    //UI组件
    private Text characterNameText;
    private TextMeshProUGUI contentText;
    private string fullText;
    private DialogueOptionUI optionPrefab;
    private Transform optionsContainer;
    private Button nextButton;
    private Image Avatar;

    public event Action OnNextButtonClick;
    private Coroutine typeCoroutine;
    private float typeSpeed=0.03f;
    [SerializeField] IAvatarService avatarService;
    private void Awake()
    {
        avatarService = AvatarManager.Instance;

        //注册事件
        OnBtnClick("Panel/NextBtn", OnNextBtnClicked);

        //获取组件
        characterNameText = transform.Find("Panel/characterNameText").GetComponent<Text>();
        contentText = transform.Find("Panel/ContentText").GetComponent<TextMeshProUGUI>();
        nextButton = transform.Find("Panel/NextBtn").GetComponent<Button>();
        optionsContainer = transform.Find("Panel/OptionsContainer");
        Avatar= transform.Find("Panel/Avatars").GetComponent<Image>();

        //加载预制体
        optionPrefab = Resources.Load<GameObject>("Prefabs/Dialogue/DialogueOptionUI")
            .GetComponent<DialogueOptionUI>();
        //对象池预先加载对象
        ObjectPoolsManager.Instance.PreLoadPrefab(optionPrefab.gameObject, 5);

        HideNextButton();
        HIdeDialoguePanel();
    }
    /// <summary>
    /// 按钮触发事件
    /// </summary>
    private void OnNextBtnClicked()
    {
        OnNextButtonClick?.Invoke();
    }
    /// <summary>
    /// 显示选项
    /// </summary>
    /// <param name="options"></param>
    /// <param name="onSelect"></param>
    public void ShowOptions(List<DialogueOption> options, Action<DialogueOption> onSelect)
    {
        //回收旧的选项
        ClearContent();
        HideNextButton();

        //显示新的选项
        foreach (var option in options)
        {
          var optionUI= ObjectPoolsManager.Instance.
                Spawn(optionPrefab.gameObject,Vector3.zero,Quaternion.identity,optionsContainer);
            optionUI.GetComponent<DialogueOptionUI>().SetData(option, ()=> 
            {
                onSelect?.Invoke(option);
                ClearContent();
            });
        }
    }
    /// <summary>
    /// 显示对话
    /// </summary>
    /// <param name="data"></param>
    public async void ShowDialogue(string characterName,string content,int characterId)
    {
        ClearContent(); //清楚残留选项
        ShowNextButton();
        Avatar.sprite = await avatarService.GetAvatarByIdAsync(characterId);
        characterNameText.text = characterName;
        fullText = content;
        typeCoroutine = StartCoroutine(TypeCoroutine());
        
    }
    /// <summary>
    /// 打印文字协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator TypeCoroutine()
    {
        foreach (char c in fullText.ToCharArray())
        {
            contentText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typeCoroutine = null;
    }
    /// <summary>
    /// 清除所有选项
    /// </summary>
    private void ClearContent()
    {
        //获取所有选项并回收到对象池
        foreach (var option in optionsContainer.GetComponentsInChildren<DialogueOptionUI>())
            ObjectPoolsManager.Instance.Despawn(option.gameObject,0);
        //清除上次对话文本
        contentText.text = "";
        //characterNameText.text="";
    }
    /// <summary>
    /// 隐藏对话面板
    /// </summary>
    public void HIdeDialoguePanel()
    {
        ClearContent();
        characterNameText.text = "";
        contentText.text = "";
        HideNextButton();
        base.Hide();
    }
    /// <summary>
    /// 控制按钮的显隐
    /// </summary>
    public void ShowNextButton() => nextButton.gameObject.SetActive(true);
    public void HideNextButton() => nextButton.gameObject.SetActive(false);
}
