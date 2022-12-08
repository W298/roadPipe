using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WayPoint
{
    public GameObject[] points;

    public WayPoint(GameObject[] points)
    {
        this.points = points;
    }
}

public class Road : Cell
{
    public RoadInfo roadInfo;
    public WayPoint[] wayPointAry;

    public List<Car> carList = new List<Car>();

    public int GetWayPointIndexFrom(int from)
    {
        return roadInfo.directionAry[(int)transform.rotation.eulerAngles.z / 90].data.FindIndex(d => d.from == from);
    }

    public int GetWayPointIndexTo(int to)
    {
        return roadInfo.directionAry[(int)transform.rotation.eulerAngles.z / 90].data.FindIndex(d => d.to == to);
    }

    private void SetWayPoint()
    {
        List<WayPoint> wayPointList = new List<WayPoint>();
        for (int i = 0; i < transform.childCount; i++)
        {
            List<GameObject> l = new List<GameObject>();
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                l.Add(transform.GetChild(i).GetChild(j).gameObject);
            }
            wayPointList.Add(new WayPoint(l.ToArray()));
        }

        wayPointAry = wayPointList.ToArray();
    }

    private void Start()
    {
        SetWayPoint();
    }
}