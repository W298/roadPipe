using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BGMDB", menuName = "Scriptable Object/BGMDB", order = int.MaxValue)]
public class BGMDB : ScriptableObject
{
    [SerializeField] public List<LevelBGM> levels;
}

[Serializable]
public class LevelBGM
{
    [SerializeField] public string name;
    [SerializeField] public AudioClip defaultBGMClip;
    [SerializeField] public List<StageBGM> stageList;
}

[Serializable]
public class StageBGM
{
    [SerializeField] public int number;
    [SerializeField] public AudioClip indivisualBGMClip;
}
