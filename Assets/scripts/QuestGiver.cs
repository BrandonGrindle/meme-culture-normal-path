using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
//using UnityEditor.PackageManager;
using Unity.VisualScripting;
using static Items;
//using static UnityEditor.Progress;
using TMPro;
using UnityEngine.UI;


public class QuestGiver : MonoBehaviour, IInteractable
{
    [Header("Quest")]
    [SerializeField] private QuestInfo CurrentQuest;

    [Header("UI QUEST")]
    private TextMeshProUGUI questInfo;
    private TextMeshProUGUI progress;

    [Header("Quest Item Type")]
    [SerializeField] private Items KeyItem;
    [SerializeField] private ItemType Type;

    private string QuestID;
    private QuestState currentQuestState;

    private QuestIcons icons;

    public AudioSource source;
    public AudioClip[] greetings;
    public AudioClip[] completion;

    private GameObject Panel;
    private TextMeshProUGUI dialogue;
    public string text;

    public int UIpopuptime = 3;
    private void Awake()
    {
        QuestID = CurrentQuest.id;
        icons = GetComponentInChildren<QuestIcons>();
        questInfo = GameObject.Find("Info").GetComponent<TextMeshProUGUI>();
        progress = GameObject.Find("Progress").GetComponent<TextMeshProUGUI>();
        questInfo.text = string.Empty; progress.text = string.Empty;

        dialogue = GameObject.Find("Dialogue").GetComponent<TextMeshProUGUI>();
        dialogue.text = string.Empty;

        Panel = GameObject.Find("SpeechBox");
    }

    private void OnEnable()
    {
        EventManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }
    private void OnDisable()
    {
        EventManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    public IEnumerator Uidelay()
    {
        Debug.Log("delay Started");
        dialogue.text = text;
        Panel.SetActive(true);
        yield return new WaitForSeconds(UIpopuptime);
        dialogue.text = string.Empty;
        Panel.SetActive(false);
        Debug.Log("delay finished");
    }

    private void QuestStateChange(Quests quests)
    {
        if (quests.QuestData.id.Equals(QuestID))
        {
            currentQuestState = quests.currentProgression;
            icons.SetState(currentQuestState);
            Debug.Log("Quest with id: " + QuestID + " updated to state " + currentQuestState);
        }
    }

    public void ItemSubmissionCheck()
    {
        if (InventoryManager.Instance.HasQuestItem(Type))
        {
            foreach (Items item in InventoryManager.Instance.items)
            {
                if (item.itemType == Type)
                {
                    InventoryManager.Instance.RemoveItem(item);
                }
            }
        }
    }

    public void Interact()
    {
        //Debug.Log("Hello");
        if (currentQuestState.Equals(QuestState.CAN_START))
        {
            if (KeyItem != null)
            {
                InventoryManager.Instance.AddItem(KeyItem);                
            }
            foreach (GameObject step in CurrentQuest.steps)
            {
                QuestStep currstep = step.GetComponent<QuestStep>();
                if (currstep != null)
                {
                    questInfo.text = currstep.GetDetails();
                }
            }

            if (greetings.Length > 0)
            {
                int index = Random.Range(0, greetings.Length);
                source.clip = greetings[index];
                source.volume = 1.0f;
                source.Play();
            }
            StartCoroutine(Uidelay());
            EventManager.Instance.questEvents.StartQuest(QuestID);
        }
        else if (currentQuestState.Equals(QuestState.COMPLETE))
        {
            questInfo.text = string.Empty;
            progress.text = string.Empty;
            if (completion.Length > 0)
            {
                int index = Random.Range(0, completion.Length);
                source.clip = completion[index];
                source.volume = 1.0f; // Adjust this value as needed
                source.Play();
            }
            EventManager.Instance.questEvents.QuestComplete(QuestID);
            ItemSubmissionCheck();
        }
        else
        {
            ItemSubmissionCheck();
        }


    }
}
