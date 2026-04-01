#### 对话管理器(DialogueManager)

- 核心数据：当前对话对组、当前对话章节Id、对话数据列表
```csharp
    private DialogueGroup _currentGroup;
    private int _currentChapterId; //章节Id
    //当前组内所有的对话数据
    private List<DialogueData> _currentGroupDatas;
    //当前对话索引
    private int _currentDialogueIndex; 
```
---
- 显示当前对话(ShowCurrentDialogue)
```csharp
private void ShowCurrentDialogue()
    {
        //当前对话组完成
        if (_currentDialogueIndex == _currentGroupDatas.Count-1)
        {
            Debug.Log("数量 "+ _currentGroupDatas.Count);
            int currentGroupId = _currentGroup.id;
            if (DialogueDataModel.dialogueOptionMap.TryGetValue(currentGroupId, out var options) && options.Count > 0)
            {
                var currentData1 = _currentGroupDatas[_currentDialogueIndex];
                _eventBus.Publish(new DialogueShowEvent
                {
                    characterName = currentData1.characterName,
                    content = currentData1.content,
                    characterId = currentData1.characterId
                });

                _eventBus.Publish(new DialogueShowOptionsEvent
                {
                    options = options,
                    onSelect = _onGroupDialogueFinished
                });
                return;
            }
            LoadGroup(_currentGroup.nextGroupId);
            return;
        }

        //继续对话
        var currentData = _currentGroupDatas[_currentDialogueIndex];
        _eventBus.Publish(new DialogueShowEvent
        {
            characterName = currentData.characterName,
            content = currentData.content,           
            characterId = currentData.characterId
        });

        //保存当前对话组的对话信息
        if (_dialogueFactory.GetChapter(_currentChapterId).needSyncServer)
        {
            DialogueUpdateRequest res = new DialogueUpdateRequest();
            res.Id = GameApp.playerId;
            res.ChapterId = _currentChapterId;
            res.GroupId = _currentGroup.id;
            NetClient.Send(res);
        }
    }
```
- 如果当前对话组结束，则发布对话跳转和显示下个对话事件并加载新的对话组和发送对话同步请求到服务器；
- 如果当前对话组没有结束，则更新对话索引和发布显示下个对话事件；
---
#### DialogueSystem(连接对话逻辑更新和UI视图显示)

- 发起对话请求(StartNetDialogueRequest)
```csharp
public void StartNetDialogueRequest(int chapterId)
    {
        //发起对话请求
        DialogueRequest res = new DialogueRequest();
        res.Id = GameApp.playerId;
        res.ChapterId = chapterId;
        res.GroupId = _dialogueFactory.GetChapter(chapterId).firstGroupId;
        NetClient.Send(res);
    }
```
- 编写protobuf网络传输数据，玩家Id、对话章节Id、对话组Id，然后请求服务器开启对话；
---
- 对话请求响应(DialogueResponse)
-
```csharp
private void _DialogueResponse(Connection conn, DialogueResponse msg)
    {
        Debug.Log($"对话响应结果{msg.IsCompleted}");
        if (msg.IsCompleted)
        {
            Debug.Log(msg.ErrorMsg);
            return;
        }
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //开启对话
            StartDialogue(msg.ChapterId);
        });
    }
```
- 收到服务器的对话请求响应后判断是否请求成功，如果成功则在主线程中开启对话(通过UIManager创建对话面板)；
---
