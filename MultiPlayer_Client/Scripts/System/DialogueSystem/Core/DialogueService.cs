using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueService
{
    private readonly DialogueManager _dialogueManager;
    private readonly IEventBus _eventBus;
    private readonly IDialogueView _dialogueView;
    public DialogueService(DialogueManager dialogueManager,IEventBus eventBus,IDialogueView dialogueView)
    {
        this._dialogueManager = dialogueManager;
        this._eventBus = eventBus;
        this._dialogueView = dialogueView;
    }
}
