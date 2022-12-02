using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.U2D.Path.GUIFramework;
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

public class Car : MonoBehaviour
{
    [Serialize] public List<PathInfo> path = new List<PathInfo>();
    public int currentRoadIndex = 0;
    public int currentRoadRunningIndex = 0;
    public bool hasValidPath = false;

    public Point startPoint;
    public Point destinationPoint;

    public Cell prev;
    public Cell current;

    private float t = 0;
    private GridController gridController;

    public void PathFind()
    {
        var newPath = gridController.RequestPath(prev, current, destinationPoint);
        hasValidPath = newPath.Count != 0;

        if (path.Count == 0)
        {
            path = newPath;
            return;
        }

        path.RemoveRange(currentRoadIndex + 1, path.Count - currentRoadIndex - 1);
        if (hasValidPath) path.AddRange(newPath);
    }

    public void StartMove()
    {
        if (!hasValidPath)
        {
            var next = FindConnectedRoad(current);
            if (next.road == null || next.attachIndex == -1) return;

            path.Add(next);
            prev = current;
            current = next.road;
        }
        else
        {
            prev = current;
            current = path[0].road;
        }

        StartCoroutine(Move());
    }

    public void OnRotate()
    {
        PathFind();
    }

    private IEnumerator Move()
    {
        var road = path[currentRoadIndex].road;
        var index = path[currentRoadIndex].attachIndex == -1 ? currentRoadRunningIndex : path[currentRoadIndex].attachIndex;

        currentRoadRunningIndex = index;

        if (road.wayPointAry[index].points.Length == 2)
        {
            while (t < 1)
            {
                transform.position = Linear(road.wayPointAry[index].points[0].transform.position, road.wayPointAry[index].points[1].transform.position, t);
                t += 0.01f;

                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            while (t < 1)
            {
                var originalPosition = transform.position;
                transform.position = Bezier(road.wayPointAry[index].points[0].transform.position,
                    road.wayPointAry[index].points[1].transform.position,
                    road.wayPointAry[index].points[2].transform.position, t);
                transform.right = transform.position - originalPosition;

                t += 0.01f;
                yield return new WaitForFixedUpdate();
            }
        }

        currentRoadIndex++;
        t = 0;

        if (!hasValidPath)
        {
            var next = FindConnectedRoad(current);
            if (next.road == null || next.attachIndex == -1) yield break;

            path.Add(next);
            prev = current;
            current = next.road;
            StartCoroutine(Move());
        }

        if (hasValidPath && currentRoadIndex < path.Count)
        {
            prev = current;
            current = path[currentRoadIndex].road;
            StartCoroutine(Move());
        }
    }

    private PathInfo FindConnectedRoad(Cell targetCell)
    {
        Road nextRoad = null;
        int nextIndex = -1;
        foreach (var adjCell in targetCell.GetConnectedCell().Where(cell => !path.Exists(p => p.road == cell)))
        {
            if (adjCell is not Road) continue;
            var adjRoad = adjCell as Road;
            var adjIndex = adjRoad.GetWayPointIndexFrom(adjRoad.GetAttachedIndex(targetCell));
            if (adjIndex != -1)
            {
                nextRoad = adjRoad;
                nextIndex = adjIndex;
                break;
            }
        }

        return new PathInfo(nextRoad, nextIndex);
    }

    private static Vector2 Linear(Vector2 start, Vector2 end, float t)
    {
        return start + (end - start) * t;
    }

    private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        return (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
    }

    private void Awake()
    {
        gridController = FindObjectOfType<GridController>();
    }
}