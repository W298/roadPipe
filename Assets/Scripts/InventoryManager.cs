using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    SLOW,
    STOP
}


public class InventoryManager
{
    private Dictionary<ItemType, int> inventory;

    public InventoryManager(ItemDefaultValue[] defaultValue)
    {
        inventory = new Dictionary<ItemType, int>();
        if (defaultValue == null) return;
        foreach (var v in defaultValue)
        {
            inventory[v.type] = v.value;
        }
    }

    public void AddItem(ItemType type, int count = 1)
    {
        inventory[type] += count;
        PlayerUI.instance.UpdateItemCountUI();
    }

    public void UseItem(ItemType type, int count = 1)
    {
        inventory[type] -= count;
        if (inventory[type] < 0) inventory[type] = 0;
        PlayerUI.instance.UpdateItemCountUI();
    }

    public int GetCount(ItemType type)
    {
        return inventory[type];
    }
}
