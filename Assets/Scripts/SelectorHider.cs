using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectorHider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InputManager inputManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        inputManager.DisableCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inputManager.EnableCursor();
    }

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
    }
}
