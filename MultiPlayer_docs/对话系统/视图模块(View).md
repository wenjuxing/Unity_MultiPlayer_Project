#### 对话面板(DialoguePanel)

- 显示对话内容（ShowDialogue）

```csharp
public async void ShowDialogue(string characterName,string content,int characterId)
    {
        ClearContent(); //清楚残留选项
        ShowNextButton();
        Avatar.sprite = await avatarService.GetAvatarByIdAsync(characterId);
        characterNameText.text = characterName;
        fullText = content;
        typeCoroutine = StartCoroutine(TypeCoroutine());
        
    }
```
- 通过UIManager创建或查找面板并显示，显示面板之前先清除残留选项(跳转选项)，然后显示新的头像、文本并开启协同程序逐字打印出对话内容；
---
- 隐藏对话面板(HideDialoguePanel)

```csharp
 public void HIdeDialoguePanel()
    {
        ClearContent();
        characterNameText.text = "";
        contentText.text = "";
        HideNextButton();
        base.Hide();
    }
```
- 清除所有内容，例如选项按钮、文本、角色头像等，最后隐藏面板；
---
- 逐字打印对话内容协程
```csharp
private IEnumerator TypeCoroutine()
    {
        foreach (char c in fullText.ToCharArray())
        {
            contentText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typeCoroutine = null;
    }
```
- 把字符串转为字符数组，然后把字符以固定频率追加到显示的文本上；
