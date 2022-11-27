using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ConnectionResult
{
    public Tuple<int, int> index;
    public bool isConnected;

    public ConnectionResult(int i, int j, bool isConnected)
    {
        index = new Tuple<int, int>(i, j);
        this.isConnected = isConnected;
    }
}

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
    public WayPoint[] wayPoint;

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

        wayPoint = wayPointList.ToArray();
    }

    public ConnectionResult isConnected(Road target)
    {
        foreach (var way in wayPoint)
        {
            Vector2[] origin = new Vector2[]
            {
                way.points.First().transform.position,
                way.points.Last().transform.position
            };

            foreach (var targetWay in target.wayPoint)
            {
                Vector2[] cand = new Vector2[]
                {
                    targetWay.points.First().transform.position,
                    targetWay.points.Last().transform.position
                };

                for (int i = 0; i < origin.Length; i++)
                {
                    for (int j = 0; j < cand.Length; j++)
                    {
                        if (Vector2.Distance(origin[i], cand[j]) <= 0.001f)
                        {
                            return new ConnectionResult(i, j, true);
                        }
                    }
                }
            }
        }

        return new ConnectionResult(-1, -1, false);
    }

    protected new void Start()
    {
        base.Start();
        SetWayPoint();
    }
}
