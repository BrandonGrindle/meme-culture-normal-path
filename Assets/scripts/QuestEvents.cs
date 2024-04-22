using System;
using System.Runtime.InteropServices.WindowsRuntime;

public class QuestEvents 
{
    public event Action<string> onQuestStart;

    public void StartQuest(string ID)
    {
        if (onQuestStart != null)
        {
            onQuestStart(ID);
        }
    }

    public event Action<string> onQuestProgressed;

    public void ProgressQuest(string ID)
    {
        if (onQuestComplete != null)
        {
            onQuestProgressed(ID);
        }
    }

    public event Action<string> onQuestComplete;

    public void QuestComplete(string ID)
    {
        if (onQuestComplete != null)
        {
            onQuestComplete(ID);
        }
    }

    public event Action<Quests> onQuestStateChange;

    public void QuestStateChange(Quests quest)
    {
        if (onQuestStateChange != null)
        {
            onQuestStateChange(quest);
        }
    }

}
