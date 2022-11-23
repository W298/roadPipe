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
        bool isReverse = false;
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

        /*
        Vector2[,] thisMarginAry = new Vector2[wayPoint.Length, 2];
        for (int i = 0; i < wayPoint.Length; i++)
        {
            thisMarginAry[i, 0] = wayPoint[i].points.First().transform.position;
            thisMarginAry[i, 1] = wayPoint[i].points.Last().transform.position;
        }

        Vector2[,] targetMarginAry = new Vector2[target.wayPoint.Length, 2];
        for (int i = 0; i < target.wayPoint.Length; i++)
        {
            targetMarginAry[i, 0] = target.wayPoint[i].points.First().transform.position;
            targetMarginAry[i, 1] = target.wayPoint[i].points.Last().transform.position;
        }

        int count = 0;
        bool isReverse = false;

        for (int i = 0; i < thisMarginAry.Length; i++)
        {
            for (int ii = 0; ii < thisMarginAry.Length; ii++)
            {
                for (int j = 0; j < targetMarginAry.Length; j++)
                {
                    for (int jj = 0; jj < targetMarginAry.Length; jj++)
                    {
                        if (!(Vector2.Distance(thisMarginAry[i, ii], targetMarginAry[j, jj]) <= 0.001f)) continue;

                        isReverse = i != j;

                        count++;
                        break;
                    }
                }
            }
        }
        */
    }

    protected new void Start()
    {
        base.Start();
        SetWayPoint();
    }
}
