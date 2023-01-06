using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BGMController : MonoBehaviour
{
    private AudioSource bgmSource;
    public BGMDB bgmDB;
    public bool isLoop = true;

    private void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("Settings/MainMixer").FindMatchingGroups("Master")[0];
        bgmSource.loop = isLoop;
    }

    private void Start()
    {
        var levelBGM = bgmDB.levels.Find(level => level.name == SceneManager.GetActiveScene().name[0].ToString());
        if (levelBGM != null)
        {
            bgmSource.clip = levelBGM.defaultBGMClip;
        }

        bgmSource.Play();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        bgmSource.volume = 0f;
        float t = 0;

        while (t < 1)
        {
            bgmSource.volume = Mathf.Lerp(0, 1, t);
            t += 0.005f;
            yield return new WaitForFixedUpdate();
        }
    }
}
