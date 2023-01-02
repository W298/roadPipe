using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMController : MonoBehaviour
{
    private AudioSource bgmSource;
    public BGMDB bgmDB;
    public bool isLoop = true;

    private void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
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
    }
}
