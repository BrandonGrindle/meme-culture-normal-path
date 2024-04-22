using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Items", order = 01)]
public class Items : ScriptableObject
{
    [field: SerializeField] public string id { get; private set; }
    public string ItemName;
    public Sprite ItemSprite;
    public ItemType itemType;

    private void OnValidate()
    {
#if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public enum ItemType
    {
        FishingQuest,
        CombatQuest,
        FishingRod,
        Sword,
        Bow,
        Shield
    }
}
