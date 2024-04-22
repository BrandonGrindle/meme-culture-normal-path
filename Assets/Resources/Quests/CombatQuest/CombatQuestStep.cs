using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatQuestStep : QuestStep
{
    private int SkeletonsKilled = 0;
    private int CompletionCount = 5;

    private TextMeshProUGUI progress;
    private void OnEnable()
    {
        EventManager.Instance.cstmevents.onSkeletonKilled += SkeletonKilled;
    }

    private void OnDisable()
    {
        EventManager.Instance.cstmevents.onSkeletonKilled -= SkeletonKilled;
    }

    private void Awake()
    {
        progress = GameObject.Find("Progress").GetComponent<TextMeshProUGUI>();
        progress.text = GetProgress();
    }

    private void SkeletonKilled()
    {
        if (SkeletonsKilled < CompletionCount)
        {
            SkeletonsKilled++;
            progress.text = GetProgress();
        }

        if (SkeletonsKilled >= CompletionCount)
        {
            progress.text = "complete!";
            FinishStep();
        }
    }

    public override string GetDetails()
    {
        return $"Kill {CompletionCount} Skeletons";
    }

    public string GetProgress()
    {
        return $"{SkeletonsKilled} / {CompletionCount} enemies killed";
    }
}
