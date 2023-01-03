using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private static PlayerUI _instance;
    public static PlayerUI instance => _instance ??= FindObjectOfType<PlayerUI>();

    public GameObject pauseMenu;
    public Text stopText;
    public Text slowText;

    public Text stopKeyText;
    public Text slowKeyText;
    public Text rotateKeyText;

    public void HideStopUI()
    {
        stopText.transform.parent.gameObject.SetActive(false);

        slowText.transform.parent.GetComponent<RectTransform>().anchoredPosition =
            stopText.transform.parent.GetComponent<RectTransform>().anchoredPosition;
    }

    public void HideSlowUI()
    {
        slowText.transform.parent.gameObject.SetActive(false);
    }

    public void HideAllItemUI()
    {
        stopText.transform.parent.gameObject.SetActive(false);
        slowText.transform.parent.gameObject.SetActive(false);
        rotateKeyText.transform.parent.parent.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        stopText.text = "X " + GameManager.instance.inventoryManager.GetCount(ItemType.STOP).ToString();
        slowText.text = "X " + GameManager.instance.inventoryManager.GetCount(ItemType.SLOW).ToString();
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

    private void Awake()
    {
        stopText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        slowText = transform.GetChild(1).GetChild(0).GetComponent<Text>();
    }

    private void Start()
    {
        UpdateUI();
        if (!GameManager.instance.allowSlow)
        {
            HideSlowUI();
            if (!GameManager.instance.allowStop) HideAllItemUI();
        }
        else if (!GameManager.instance.allowStop)
        {
            HideStopUI();
        }
    }

    private void OnDestroy()
    {
        _instance = null;
        Time.timeScale = 1;
    }
}
