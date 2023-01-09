using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum BGMType
{
    NONE,
    STAGE,
    MENU
}

public class MenuBGM : MonoBehaviour
{
    private AudioSource menuAudioSource;
    private AudioSource stageAudioSource;

    private BGMType prevBGMType = BGMType.NONE;
    private BGMType currentBGMType = BGMType.NONE;

    private IEnumerator menuAudioFadeOut = null;
    private IEnumerator stageAudioFadeOut = null;

    private IEnumerator menuAudioFadeIn = null;
    private IEnumerator stageAudioFadeIn = null;

    public BGMDB stageBGMDB;
    public AudioClip menuBGM;

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (gameObject == null) return;

        prevBGMType = currentBGMType;
        currentBGMType = scene.name is "MainMenu" or "StageSelect" ? BGMType.MENU : BGMType.STAGE;

        if (prevBGMType == currentBGMType) return;

        if (currentBGMType == BGMType.MENU)
        {
            if (stageAudioSource.isPlaying) FadeOutAudio(BGMType.STAGE);
            
            menuAudioSource.clip = menuBGM;
            FadeInAudio(BGMType.MENU, 0.7f);
        }
        else
        {
            if (menuAudioSource.isPlaying) FadeOutAudio(BGMType.MENU);

            var levelBGM = stageBGMDB.levels.Find(level => level.name == scene.name[0].ToString());
            stageAudioSource.clip = levelBGM.defaultBGMClip;
            FadeInAudio(BGMType.STAGE, 1f);
        }
    }

    private void FadeInAudio(BGMType type, float maxVolume)
    {
        var source = type == BGMType.MENU ? menuAudioSource : stageAudioSource;
        var fadeOutCoroutine = type == BGMType.MENU ? menuAudioFadeOut : stageAudioFadeOut;

        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        if (type == BGMType.MENU)
        {
            menuAudioFadeIn = FadeIn(source, maxVolume);
            StartCoroutine(menuAudioFadeIn);
        }
        else
        {
            stageAudioFadeIn = FadeIn(source, maxVolume);
            StartCoroutine(stageAudioFadeIn);
        }
    }

    private void FadeOutAudio(BGMType type)
    {
        var source = type == BGMType.MENU ? menuAudioSource : stageAudioSource;
        var fadeInCoroutine = type == BGMType.MENU ? menuAudioFadeIn : stageAudioFadeIn;

        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        if (type == BGMType.MENU)
        {
            menuAudioFadeOut = FadeOut(source);
            StartCoroutine(menuAudioFadeOut);
        }
        else
        {
            stageAudioFadeOut = FadeOut(source);
            StartCoroutine(stageAudioFadeOut);
        }
    }

    private void Awake()
    {
        var another = FindObjectsOfType<MenuBGM>();
        if (another.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoad;

            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("Settings/MainMixer").FindMatchingGroups("Master")[0];
            menuAudioSource.volume = 0f;

            stageAudioSource = gameObject.AddComponent<AudioSource>();
            stageAudioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("Settings/MainMixer").FindMatchingGroups("Master")[0];
            stageAudioSource.volume = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private IEnumerator FadeIn(AudioSource source, float maxVolume)
    {
        source.Play();

        var startVolume = source.volume;
        float t = 0;

        while (t < 1)
        {
            source.volume = Mathf.Lerp(startVolume, maxVolume, t);
            t += 0.005f;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator FadeOut(AudioSource source)
    {
        var startVolume = source.volume;
        float t = 0;

        while (t < 1)
        {
            source.volume = Mathf.Lerp(startVolume, 0, t);
            t += 0.009f;
            yield return new WaitForFixedUpdate();
        }

        source.Stop();
    }
}
