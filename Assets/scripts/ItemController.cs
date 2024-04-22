using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Items item;

    public void addItem(Items NewItem)
    {
        item = NewItem;
    }

    public void useItem()
    {
        if (item != null)
        {
            switch (item.itemType)
            {
                case Items.ItemType.FishingRod:
                    ThirdPersonController.instance.EquipItem(1);
                    break;
                case Items.ItemType.Sword:
                    ThirdPersonController.instance.EquipItem(2);
                    break;

            }
        }
        else
        {
            Debug.Log("item does not exist");
        }
    }
}
