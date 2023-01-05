using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
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

[Serializable]
public class MaskInfo
{
    public Sprite sprite;
    public float t;

    public MaskInfo(Sprite sprite, float t)
    {
        this.sprite = sprite;
        this.t = t;
    }
}

[Serializable]
public class MaskInfoBunch
{
    public int index;
    public List<MaskInfo> data;

    public MaskInfoBunch(int index, List<MaskInfo> data)
    {
        this.index = index;
        this.data = data;
    }
}

public class Road : Cell
{
    private SpriteRenderer spriteRenderer;
    public List<RoadDash> dashList;

    public List<MaskInfoBunch> maskList;
    public List<GameObject> maskGameObjectList;

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

    public Sprite GetMask(float t, int index)
    {
        var vt = (float)(Math.Truncate(t * 100) / 100);

        if (vt == 0) return maskList.Count == 0 ? null : maskList[index].data[0].sprite;
        for (int i = 0; i < maskList[index].data.Count - 2; i++)
        { 
            if (vt >= maskList[index].data[i].t && vt < maskList[index].data[i + 1].t)
            {
                return maskList[index].data[i + 1].sprite;
            }
        }
        return null;
    }

    private void LoadMask()
    {
        var roadName = spriteRenderer.sprite.name;
        maskList = new List<MaskInfoBunch>();

        for (int i = 0; i < wayPointAry.Length; i++)
        {
            var list = new List<MaskInfo>();
            var tex = Resources.LoadAll("RoadMask/" + roadName + "/" + i.ToString(), typeof(Sprite));
            if (tex.Length == 0) break;
            foreach (var ts in tex)
            {
                var v = ts.name[0] + "." + ts.name[1..];
                var vf = float.Parse(v);

                list.Add(new MaskInfo(ts as Sprite, vf));
            }
            maskList.Add(new MaskInfoBunch(i, list));
        }
    }

    protected override void RotationUpdate()
    {
        base.RotationUpdate();
        maskGameObjectList.ForEach(g => g.transform.rotation = transform.rotation);
    }

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        dashList = GetComponentsInChildren<RoadDash>().ToList();
        SetWayPoint();
    }

    private void Start()
    {
        LoadMask();
    }
}