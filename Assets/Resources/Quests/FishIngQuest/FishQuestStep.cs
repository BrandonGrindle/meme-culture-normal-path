using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class FishQuestStep : QuestStep
{
    private int fishCollected = 0;
    private int CompletionCount = 3;

    private TextMeshProUGUI progress;
    private void OnEnable()
    {
        EventManager.Instance.cstmevents.onFishCollected += FishCollected;
    }

    private void OnDisable()
    {
        EventManager.Instance.cstmevents.onFishCollected -= FishCollected;
    }
    private void Awake()
    {
        progress = GameObject.Find("Progress").GetComponent<TextMeshProUGUI>();
        progress.text = GetProgress();
    }

    private void FishCollected()
    {
        if (fishCollected < CompletionCount)
        {
            fishCollected++;
            progress.text = GetProgress();
        }

        if (fishCollected >= CompletionCount)
        {
            progress.text = "complete!";
            FinishStep();
        }
    }

    public override string GetDetails()
    {
        return $"catch {CompletionCount} fish";
    }

    public string GetProgress()
    {
        return $"{fishCollected} / {CompletionCount} fish collected";
    }
}
