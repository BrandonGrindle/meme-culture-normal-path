using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    private bool stepComplete = false;

    private string QuestID;

    public abstract string GetDetails();
    public void InitialiseQuestStep(string QuestID)
    {
        this.QuestID = QuestID;
    }

    protected void FinishStep()
    {
        if (!stepComplete)
        {
            stepComplete = true;
            EventManager.Instance.questEvents.ProgressQuest(QuestID);
            Destroy(this.gameObject);
        }
    }
}
