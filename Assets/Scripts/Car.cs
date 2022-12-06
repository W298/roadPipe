using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour
{
    [Serialize] public List<PathInfo> path = new List<PathInfo>();
    public int currentRoadIndex = 0;
    public int currentRoadRunningIndex = 0;
    public bool hasValidPath = false;
    public bool arrived = false;

    public Point startPoint;
    public Point destinationPoint;

    public Road currentRoad;

    private float t = 0;
    private GridController gridController;
    private Text nameText;

    public bool PathFind(Cell start, Cell destination)
    {
        var newPath = gridController.RequestPath(start, destination);
        hasValidPath = newPath.Count != 0;

        if (!hasValidPath)
        {
            return false;
        }

        if (path.Count == 0)
        {
            OverridePath(newPath);
        }
        else
        {
            ReplacePath(newPath);
        }

        return true;
    }

    public void StartMove()
    {
        PathFind(startPoint, destinationPoint);

        if (!hasValidPath)
        {
            var next = FindConnectedRoad(startPoint);
            if (next.road == null || next.attachIndex == -1) return;

            path.Add(next);
        }

        StartCoroutine(Move());
    }

    public void OnRotate()
    {
        if (NeedPathFind()) PathFind(currentRoad, destinationPoint);
    }

    private void OverridePath(List<PathInfo> newPath)
    {
        path = newPath;
    }

    private void ClearAfterPath()
    {
        path.RemoveRange(currentRoadIndex + 1, path.Count - currentRoadIndex - 1);
    }

    private void ReplacePath(List<PathInfo> newPath)
    {
        ClearAfterPath();
        path.AddRange(newPath);
    }

    private bool NeedPathFind()
    {
        return !arrived && !destinationPoint.isConnected(currentRoad);
    }

    private bool SetNextRoad()
    {
        currentRoadIndex++;
        t = 0;

        if (!hasValidPath)
        {
            var next = FindConnectedRoad(currentRoad);
            if (next.road == null || next.attachIndex == -1)
            {
                DestroyCar();
                return false;
            }

            path.Add(next);
        }
        else
        {
            if (currentRoadIndex >= path.Count)
            {
                ParkCar();
                return false;
            }
        }

        return true;
    }

    private IEnumerator Move()
    {
        currentRoad = path[currentRoadIndex].road;
        currentRoadRunningIndex = path[currentRoadIndex].attachIndex == -1 ? currentRoadRunningIndex : path[currentRoadIndex].attachIndex;

        if (currentRoad.wayPointAry[currentRoadRunningIndex].points.Length == 2)
        {
            while (t < 1)
            {
                transform.position = Linear(currentRoad.wayPointAry[currentRoadRunningIndex].points[0].transform.position, currentRoad.wayPointAry[currentRoadRunningIndex].points[1].transform.position, t);
                t += 0.01f;

                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            var start = currentRoad.wayPointAry[currentRoadRunningIndex].points[0].transform.position;
            var control = currentRoad.wayPointAry[currentRoadRunningIndex].points[1].transform.position;
            var end = currentRoad.wayPointAry[currentRoadRunningIndex].points[2].transform.position;
            var a = start + (control - start).normalized * 0.15f;
            var b = end + (control - end).normalized * 0.15f;

            while (t < 1)
            {
                var originalPosition = transform.position;
                switch (t)
                {
                    case <= 0.2f:
                        transform.position = Linear(start, a, t * 5);
                        break;
                    case >= 0.8f:
                        transform.position = Linear(b, end, (t - 0.8f) * 5);
                        break;
                    default:
                        transform.position = Bezier(a, control, b, (t - 0.2f) * (10f / 6f));
                        transform.right = transform.position - originalPosition;
                        break;
                }

                t += 0.005f / Vector2.Distance(a, b) * 0.85f;
                yield return new WaitForFixedUpdate();
            }
        }

        var needNextMove = SetNextRoad();
        if (needNextMove) StartCoroutine(Move());
    }

    private PathInfo FindConnectedRoad(Cell targetCell)
    {
        Road nextRoad = null;
        int nextIndex = -1;

        Cell[] searchPath = targetCell.GetConnectedCell();
        foreach (var adjCell in searchPath)
        {
            if (path.Exists(p => p.road == adjCell) && path.FindIndex(p => p.road == adjCell) < currentRoadIndex) continue;

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

    private void ParkCar()
    {
        arrived = true;
    }

    private void DestroyCar()
    {
        Destroy(gameObject);
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
        nameText = GetComponentInChildren<Text>();
    }

    private void Start()
    {
        nameText.text = gameObject.name;
    }
}