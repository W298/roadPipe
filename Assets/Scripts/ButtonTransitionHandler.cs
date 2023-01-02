using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTransitionHandler : MonoBehaviour
{
    private EventTrigger trigger;

    public List<Image> effectImages;
    public List<Text> effectTexts;

    public Color defaultColor;
    public Color hoverColor;
    public Color pressedColor;

    private void OnHover()
    {
        effectImages.ForEach(image =>
        {
            image.color = hoverColor;
        });

        effectTexts.ForEach(text =>
        {
            text.color = hoverColor;
        });
    }

    private void OnHoverEnd()
    {
        effectImages.ForEach(image =>
        {
            image.color = defaultColor;
        });

        effectTexts.ForEach(text =>
        {
            text.color = defaultColor;
        });
    }

    private void OnClick()
    {
        OnHover();
    }

    private void Awake()
    {
        trigger = GetComponent<EventTrigger>();
        if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();

        var pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener(data =>
        {
            OnHover();
        });

        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener(data =>
        {
            OnHoverEnd();
        });

        var pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener(data =>
        {
            OnClick();
        });

        trigger.triggers.Add(pointerEnter);
        trigger.triggers.Add(pointerExit);
        trigger.triggers.Add(pointerClick);

        OnHoverEnd();
    }
}
