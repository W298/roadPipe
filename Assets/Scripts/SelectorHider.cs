using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectorHider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        InputManager.instance.DisableCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InputManager.instance.EnableCursor();
    }
}
