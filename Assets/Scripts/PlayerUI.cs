using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemType<T>
{
    public ItemType type;
    public T value;

    public ItemType(ItemType type, T value)
    {
        this.type = type;
        this.value = value;
    }
}

public class PlayerUI : MonoBehaviour
{
    private static PlayerUI _instance;
    public static PlayerUI instance => _instance ??= FindObjectOfType<PlayerUI>();

    private List<ItemType<Sprite>> disableTextureList;

    public GameObject pauseMenu;

    public List<ItemType<Text>> textList;
    public List<ItemType<Image>> imageList;
    public List<ItemType<GameObject>> bgList;

    public Text stopKeyText;
    public Text slowKeyText;
    public Text rotateKeyText;

    public GameObject rotateBG;

    public void UpdateItemCountUI()
    {
        foreach (var itemType in textList)
        {
            itemType.value.text = "X " + GameManager.instance.inventoryManager.GetCount(itemType.type);
        }
    }

    public void UpdateKeyUI(string stopKey, string slowKey, string rotateKey)
    {
        stopKeyText.text = stopKey;
        slowKeyText.text = slowKey;
        rotateKeyText.text = rotateKey;
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu.activeSelf)
        {
            DisablePauseMenu();
        }
        else
        {
            EnablePauseMenu();
        }
    }

    public void EnablePauseMenu()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        InputManager.instance.DisableCursor();
    }

    public void DisablePauseMenu()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        InputManager.instance.EnableCursor();
    }

    public void UpdateSelectItemUI(InputMode mode)
    {
        foreach (var itemType in bgList)
        {
            itemType.value.SetActive(false);
        }
        rotateBG.SetActive(false);

        if (mode == InputMode.ROTATE)
        {
            rotateBG.SetActive(true);
            return;
        }

        var type = (ItemType)(mode - 1);
        bgList.Find(itemType => itemType.type == type).value.SetActive(true);
    }

    private void Awake()
    {
        var tex = Resources.LoadAll("UI/Disable", typeof(Sprite));
        if (tex.Length == 0) return;

        disableTextureList = new List<ItemType<Sprite>>();
        foreach (var ts in tex)
        {
            var type = (ItemType)(int.Parse(ts.name));
            disableTextureList.Add(new ItemType<Sprite>(type, ts as Sprite));
        }
    }

    private void Start()
    {
        UpdateItemCountUI();

        foreach (var defaultValue in GameManager.instance.defaultValue)
        {
            if (!defaultValue.allow)
            {
                imageList.Find(i => i.type == defaultValue.type).value.sprite =
                    disableTextureList.Find(i => i.type == defaultValue.type).value;
            }
        }
    }

    private void OnDestroy()
    {
        _instance = null;
        Time.timeScale = 1;
    }
}
