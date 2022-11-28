using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PathInfo
{
    public Road road;
    public int attachIndex;

    public PathInfo(Road road, Cell prev)
    {
        this.road = road;
        this.attachIndex = road.GetWayPointIndexFrom(road.GetAttachedIndex(prev));
    }

    public PathInfo(Road road, int attachIndex)
    {
        this.road = road;
        this.attachIndex = attachIndex;
    }
}

public class Car : MonoBehaviour
{
    [Serialize] public List<PathInfo> path = new List<PathInfo>();
    public int currentRoadIndex = 0;
    public int currentPointIndex = 0;

    public Point startPoint;
    public Point destinationPoint;

    public void PathFind()
    {
        path.Clear();

        Cell prev = startPoint;
        Cell current = startPoint.GetAdjacentCellNotNull()[0];
        while (current is not Point || (Point)current != destinationPoint)
        {
            path.Add(new PathInfo(current as Road, prev));
            var adj = current.GetAdjacentCellNotNull().Where(cell => cell != prev).ToArray();
            if (adj.Length == 0) break;
            prev = current;
            current = adj[0];
        }

        StartCoroutine(Move());
    }

    public void OnRotate()
    {
        PathFind();
    }

    private IEnumerator Move()
    {
        yield return new WaitForSeconds(1f);

        var road = path[currentRoadIndex].road;
        var index = path[currentRoadIndex].attachIndex;

        transform.position = road.wayPointAry[index].points[currentPointIndex].transform.position;

        currentPointIndex++;
        if (currentPointIndex >= road.wayPointAry[index].points.Length)
        {
            currentRoadIndex++;
            currentPointIndex = 1;
        }
        if (currentRoadIndex < path.Count) StartCoroutine(Move());
    }
}
