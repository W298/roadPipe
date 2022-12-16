using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private static PlayerUI _instance;

    public static PlayerUI instance
    {
        get
        {
            _instance ??= FindObjectOfType<PlayerUI>();
            return _instance;
        }
    }

    public Text stopText;
    public Text slowText;

    public void UpdateUI()
    {
        stopText.text = "X " + GameManager.instance.inventoryManager.GetCount(ItemType.STOP).ToString();
        slowText.text = "X " + GameManager.instance.inventoryManager.GetCount(ItemType.SLOW).ToString();
    }

    private void Awake()
    {
        stopText = transform.GetChild(0).GetComponentInChildren<Text>();
        slowText = transform.GetChild(1).GetComponentInChildren<Text>();

        UpdateUI();
    }
}
