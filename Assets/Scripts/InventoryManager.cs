using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    private Dictionary<ItemType, int> inventory;

    public InventoryManager(int defStopCount = 0, int defSlowCount = 0)
    {
        inventory = new Dictionary<ItemType, int>();
        inventory[ItemType.STOP] = defStopCount;
        inventory[ItemType.SLOW] = defSlowCount;
    }

    public void AddItem(ItemType type, int count = 1)
    {
        inventory[type] += count;
        PlayerUI.instance.UpdateUI();
    }

    public void UseItem(ItemType type, int count = 1)
    {
        inventory[type] -= count;
        if (inventory[type] < 0) inventory[type] = 0;
        PlayerUI.instance.UpdateUI();
    }

    public int GetCount(ItemType type)
    {
        return inventory[type];
    }
}
