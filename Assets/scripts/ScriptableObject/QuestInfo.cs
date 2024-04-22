using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfo", menuName = "ScriptableObjects/QuestInfo", order = 1)]
public class QuestInfo : ScriptableObject
{
    [field: SerializeField] public string id { get; private set; }

    [Header("General")]
    public string Name;

    [Header("steps")]
    public GameObject[] steps;

    [Header("prior quests before activation")]
    public QuestInfo[] questPreReq;

    [Header("rewards")]

    public int reward;
    private void OnValidate()
    {
#if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
