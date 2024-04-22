using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Properties;
using UnityEngine;

public class QuestIcons : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private GameObject QuestAvailable;
    [SerializeField] private GameObject QuestComplete;

    public void SetState(QuestState state)
    {
        QuestAvailable.SetActive(false); 
        QuestComplete.SetActive(false);

        switch (state)
        {
            case QuestState.CAN_START: 
                QuestAvailable.SetActive(true); 
                break;
            case QuestState.IN_PROGRESS: 
                QuestAvailable.SetActive(false); 
                break;
                case QuestState.COMPLETE: 
                QuestComplete.SetActive(true);
                break;
                case QuestState.SUBMITTED: 
                QuestComplete.SetActive(false); 
                break;
            default:
                QuestAvailable.SetActive(false); 
                QuestComplete.SetActive(false);
                break;

        }
    }
}
