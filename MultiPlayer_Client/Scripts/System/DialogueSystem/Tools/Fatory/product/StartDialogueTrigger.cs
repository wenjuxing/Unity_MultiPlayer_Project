using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StartDialogueTrigger : IEventTrigger
{
    private readonly string chapterId;
    public StartDialogueTrigger(string chapterId)
    {
        this.chapterId = chapterId;
    }
    public void Execute()
    {
        DialogueSystem.Instance.StartNetDialogueRequest(int.Parse(chapterId));
    }
}