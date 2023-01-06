using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBGM : MonoBehaviour
{
    private AudioSource audioSource;

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu" && scene.name != "StageSelect" && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        var another = FindObjectsOfType<MenuBGM>();
        if (another.Length != 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoad;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        float t = 0;

        while (t < 1)
        {
            audioSource.volume = Mathf.Lerp(0, 0.7f, t);
            t += 0.005f;
            yield return new WaitForFixedUpdate();
        }
    }
}
