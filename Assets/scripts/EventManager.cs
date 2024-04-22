using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public CustomEvents cstmevents;

    public QuestEvents questEvents;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("more than one event manager present");
        }
        else
        {
            Debug.Log("EventManager instance was created.");
        }
        Instance = this;

        cstmevents = new CustomEvents();
        questEvents = new QuestEvents();
    }
}
