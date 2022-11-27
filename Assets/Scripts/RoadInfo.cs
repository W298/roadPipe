using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoadInfo", menuName = "Scriptable Object/RoadInfo", order = int.MaxValue)]
public class RoadInfo : ScriptableObject
{
    [SerializeField] public DirectionInfo[] directionAry = new DirectionInfo[4];
}

[Serializable]
public struct DirectionInfo
{
    [SerializeField] public List<DirectionUnit> data;
}

[Serializable]
public struct DirectionUnit
{
    [SerializeField] public int from;
    [SerializeField] public int to;
}