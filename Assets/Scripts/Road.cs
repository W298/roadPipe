using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class WayPoint
{
    public GameObject[] points;

    public WayPoint(GameObject[] points)
    {
        this.points = points;
    }
}

public class BezierRendererBunch
{
    public BezierRenderer bezierRenderer;
    public List<GameObject> points;

    public BezierRendererBunch(BezierRenderer bezierRenderer, List<GameObject> points)
    {
        this.bezierRenderer = bezierRenderer;
        this.points = points;
    }
}

public class Road : Cell
{
    private List<BezierRendererBunch> bezierRendererList;

    public RoadInfo roadInfo;
    public WayPoint[] wayPointAry;
    public List<Car> carList = new List<Car>();

    public int GetWayPointIndexFrom(int from)
    {
        return roadInfo.directionAry[rotation].data.FindIndex(d => d.from == from);
    }

    public int GetWayPointIndexTo(int to)
    {
        return roadInfo.directionAry[rotation].data.FindIndex(d => d.to == to);
    }

    private void SetWayPoint()
    {
        List<WayPoint> wayPointList = new List<WayPoint>();
        for (int i = 0; i < transform.childCount / 2; i++)
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

    public void InitPathRender(Car car, int attachIndex)
    {
        var color = car.startPoint.pointTheme.color;
        bezierRendererList[attachIndex].bezierRenderer.color = color;
        UpdatePathRender(attachIndex, 0);
    }

    public void ResetPathRender(int attachIndex)
    {
        UpdatePathRender(attachIndex, 1);
    }

    public void UpdatePathRender(int attachIndex, float t)
    {
        var bezierRendererBunch = bezierRendererList[attachIndex];
        var bezierRenderer = bezierRendererBunch.bezierRenderer;
        var points = bezierRendererBunch.points;

        if (points.Count >= 3)
        {
            bezierRenderer.start = points[0].transform.position + (points[1].transform.position - points[0].transform.position).normalized * 0.15f;
            bezierRenderer.control = points[1].transform.position;
            bezierRenderer.end = points[2].transform.position + (points[1].transform.position - points[2].transform.position).normalized * 0.15f;

            bezierRenderer.forward = points[0].transform.position;
            bezierRenderer.backward = points[2].transform.position;
        }
        else
        {
            bezierRenderer.isLinear = true;
            bezierRenderer.start = points[0].transform.position;
            bezierRenderer.end = points[1].transform.position;
        }

        bezierRenderer.t = t;
        bezierRenderer.Render();
    }

    protected override void RotationUpdate()
    {
        base.RotationUpdate();

        for (int i = 0; i < bezierRendererList.Count; i++)
        {
            if (bezierRendererList[i].bezierRenderer.t is 0 or 1)
            {
                UpdatePathRender(i, bezierRendererList[i].bezierRenderer.t);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        bezierRendererList = GetComponentsInChildren<BezierRenderer>().Select(bezierRenderer =>
        {
            var points = new List<GameObject>();
            for (int i = 0; i < bezierRenderer.transform.childCount; i++)
            {
                points.Add(bezierRenderer.transform.GetChild(i).gameObject);
            }

            return new BezierRendererBunch(bezierRenderer, points);
        }).ToList();
        SetWayPoint();
    }

    private void Start()
    {
        for (int i = 0; i < bezierRendererList.Count; i++)
        {
            UpdatePathRender(i, 1);
        }
    }
}