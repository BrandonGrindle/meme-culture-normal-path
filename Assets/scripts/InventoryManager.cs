using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;
using System.Linq;
//using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Items> items = new List<Items>();

    public Transform ItemContent;
    public GameObject ItemContainer;

    public List<ItemController> ItemController = new List<ItemController>();
    private void Awake()
    {
        Instance = this;
    }

    public void AddItem(Items item)
    {
        items.Add(item);
    }

    public void RemoveItem(Items item)
    {
        items.Remove(item);
    }

    public void listItems()
    {
        foreach (var item in items)
        {
            GameObject obj = Instantiate(ItemContainer, ItemContent);
            var ItemName = obj.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            var ItemIcon = obj.transform.Find("ItemIcon").GetComponent<Image>();



            ItemName.text = item.ItemName;
            ItemIcon.sprite = item.ItemSprite;
        }

        SetinvItems();
    }

    public void SetinvItems()
    {
        ItemController = ItemContent.GetComponentsInChildren<ItemController>().ToList();


        for (int i = 0; i < items.Count && i < ItemController.Count; i++)
        {
            ItemController[i].addItem(items[i]);
        }
    }

    public bool HasQuestItem(Items.ItemType ITEMTYPE)
    {
        return items.Any(item => item.itemType == ITEMTYPE);
    }

    public void ClearList()
    {
        foreach (Transform item in ItemContent)
        {
            Destroy(item.gameObject);

        }
    }

}
