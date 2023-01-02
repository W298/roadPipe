using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSFXHandler : MonoBehaviour
{
    private EventTrigger trigger;
    private AudioSource audioSource;

    private void Awake()
    {
        trigger = GetComponent<EventTrigger>();
        if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = Resources.Load<AudioClip>("SFX/click_sfx");

        var pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener(data =>
        {
            audioSource.pitch = 1.3f;
            audioSource.Play();
        });

        var pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener(data =>
        {
            if (!gameObject.activeInHierarchy) return;
            audioSource.pitch = 1;
            audioSource.Play();
        });
        
        trigger.triggers.Add(pointerEnter);
        trigger.triggers.Add(pointerClick);
    }
}
