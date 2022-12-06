using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathInfo
{
    public Road road;
    public int attachIndex;

    public PathInfo(Road road, Cell prev)
    {
        this.road = road;
        SetAttachIndex(prev);
    }

    public PathInfo(Road road, int attachIndex)
    {
        this.road = road;
        this.attachIndex = attachIndex;
    }

    public void SetAttachIndex(Cell prev)
    {
        attachIndex = road.GetWayPointIndexFrom(road.GetAttachedIndex(prev));
    }
}
