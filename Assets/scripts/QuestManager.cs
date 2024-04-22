using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private Dictionary<string, Quests> questMap;

    private void Awake()
    {
        questMap = CreatequestMap();
    }

    private void OnEnable()
    {
        EventManager.Instance.questEvents.onQuestStart += StartQuest;
        EventManager.Instance.questEvents.onQuestProgressed += AdvanceQuest;
        EventManager.Instance.questEvents.onQuestComplete += CompleteQuest;


    }

    private void Start()
    {
        foreach (Quests quests in questMap.Values)
        {
            EventManager.Instance.questEvents.QuestStateChange(quests);
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.questEvents.onQuestStart -= StartQuest;
        EventManager.Instance.questEvents.onQuestProgressed -= AdvanceQuest;
        EventManager.Instance.questEvents.onQuestComplete -= CompleteQuest;
    }

    private void StartQuest(string id)
    {
        Quests quest = GetQuestsbyId(id);
        quest.InstantiateQuestStep(this.transform);
        ChangeQuestState(quest.QuestData.id, QuestState.IN_PROGRESS);
    }

    private void AdvanceQuest(string id)
    {
        Quests quest = GetQuestsbyId(id);

        quest.ProgressStep();

        if (quest.ValidCurrentStep())
        {
            quest.InstantiateQuestStep(this.transform);
        }
        else
        {
            ChangeQuestState(quest.QuestData.id, QuestState.COMPLETE);
        }
    }

    private void CompleteQuest(string id)
    {
        Quests quest = GetQuestsbyId(id);
        ChangeQuestState(quest.QuestData.id, QuestState.SUBMITTED);
    }

    private void ChangeQuestState(string id, QuestState State)
    {
        Quests quest = GetQuestsbyId(id);
        quest.currentProgression = State;
        EventManager.Instance.questEvents.QuestStateChange(quest);

    }

    private bool requirementsCheck(Quests quests)
    {
        bool requirements = true;

        foreach (QuestInfo quest in quests.QuestData.questPreReq)
        {
            if (GetQuestsbyId(quest.id).currentProgression != QuestState.SUBMITTED)
            {
                requirements = false;
            }
        }

        return requirements;
    }

    private void Update()
    {
        foreach (Quests quest in questMap.Values)
        {
            if (quest.currentProgression == QuestState.REQUIREMENTS_NOT_MET && requirementsCheck(quest))
            {
                ChangeQuestState(quest.QuestData.id, QuestState.CAN_START);
            }
        }
    }
    private Dictionary<string, Quests> CreatequestMap()
    {
        QuestInfo[] questInfos = Resources.LoadAll<QuestInfo>("Quests");

        Dictionary<string, Quests> QuestIDMap = new Dictionary<string, Quests>();

        foreach (QuestInfo questInfo in questInfos)
        {
            if (QuestIDMap.ContainsKey(questInfo.id))
            {
                Debug.LogWarning("dupicate key detected when creatying quest map: " + questInfo.id);
            }

            QuestIDMap.Add(questInfo.id, new Quests(questInfo));
        }
        return QuestIDMap;
    }

    private Quests GetQuestsbyId(string id)
    {
        Quests quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("Quest id not found in quest map: " + id);
        }
        return quest;
    }
}
