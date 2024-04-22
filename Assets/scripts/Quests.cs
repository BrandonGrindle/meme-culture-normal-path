using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quests
{
    public QuestInfo QuestData;

    public QuestState currentProgression;
    public int QuestStepIndex;

    public Quests(QuestInfo Quest)
    {
        this.QuestData = Quest;
        this.currentProgression = QuestState.REQUIREMENTS_NOT_MET;
        this.QuestStepIndex = 0;

    }

    public void ProgressStep()
    {
        QuestStepIndex++;
    }

    public bool ValidCurrentStep()
    {
        return (QuestStepIndex < QuestData.steps.Length);
    }

    public void InstantiateQuestStep(Transform ParentTransform)
    {
        GameObject QuestStepPrefab = GetCurrentQuestStepPrefab();
        if (QuestStepPrefab != null)
        {
            QuestStep questStep = Object.Instantiate<GameObject>(QuestStepPrefab, ParentTransform).GetComponent<QuestStep>();
            questStep.InitialiseQuestStep(QuestData.id);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepprefab = null;
        if (ValidCurrentStep())
        {
            questStepprefab = QuestData.steps[QuestStepIndex];
        }
        else
        {
            Debug.LogWarning("no current quest steps for the quest " + QuestData.id);
        }
        return questStepprefab;
    }
}
