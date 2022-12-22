using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum NextRoadStatusType
{
    VALID,
    ADJACENT,
    DISCONNECTED,
    DESTINATION
}

public struct NextRoadStatus
{
    public NextRoadStatusType statusType;
    public PathInfo roadInfo;

    public NextRoadStatus(NextRoadStatusType statusType, PathInfo roadInfo = null)
    {
        this.statusType = statusType;
        this.roadInfo = roadInfo;
    }
}

public class Car : MonoBehaviour
{
    [Serialize] public List<PathInfo> path = new List<PathInfo>();
    public int currentRoadIndex = 0;
    public int currentRoadRunningIndex = -1;
    public bool hasValidPath = false;
    public bool arrived = false;

    public Point startPoint;
    public Point destinationPoint;

    private Road _currentRoad;
    public Road currentRoad
    {
        get => _currentRoad;

        set
        {
            _currentRoad?.carList.Remove(this);
            _currentRoad = value;
            _currentRoad?.carList.Add(this);
        }
    }

    public float t = 0;
    private float speed = 0.015f;
    private SpriteRenderer backgroundSprite;

    public bool PathFind(int startRunningIndex, Cell start, Cell destination)
    {
        foreach (var pathInfo in path.Where((info, index) => currentRoadIndex >= 0 && index >= currentRoadIndex))
        {
            pathInfo.road.ResetPathRender(pathInfo.attachIndex);
        }

        var newPath = GridController.instance.RequestPath(startRunningIndex, start, destination);
        hasValidPath = newPath.Count != 0;

        if (!hasValidPath)
        {
            ClearAfterPath();
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

        foreach (var pathInfo in path.Where((info, index) => index >= currentRoadIndex))
        {
            pathInfo.road.InitPathRender(this, pathInfo.attachIndex);
        }

        return true;
    }

    public void StartMove()
    {
        PathFind(-100, startPoint, destinationPoint);

        if (!hasValidPath)
        {
            var next = FindConnectedRoad(startPoint);
            if (next.road == null || next.attachIndex == -1) return;

            path.Add(next);
        }

        StartCoroutine(MoveRoutine());
    }

    public void OnRotate()
    {
        if (!NeedPathFind()) return;
        PathFind(currentRoadRunningIndex, currentRoad != null ? currentRoad : startPoint, destinationPoint);
    }

    private void OverridePath(List<PathInfo> newPath)
    {
        path = newPath;
    }

    private void ClearAfterPath()
    {
        if (path.Count < currentRoadIndex + 1) return;
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

    private NextRoadStatus GetNextRoadStatus()
    {
        if (hasValidPath)
            return currentRoadIndex + 1 >= path.Count
                ? new NextRoadStatus(NextRoadStatusType.DESTINATION)
                : new NextRoadStatus(NextRoadStatusType.VALID);
        
        var next = FindConnectedRoad(currentRoad);
        if (next.road == null || next.attachIndex == -1)
            return new NextRoadStatus(NextRoadStatusType.DISCONNECTED);

        return 
            next.road == destinationPoint 
                ? new NextRoadStatus(NextRoadStatusType.DESTINATION) 
                : new NextRoadStatus(NextRoadStatusType.ADJACENT, next);
    }

    public void SlowDown()
    {
        speed = 0.005f;
    }

    public void ResetSpeed()
    {
        speed = 0.015f;
    }

    private bool CheckStopEffector()
    {
        var nextRoadStatus = GetNextRoadStatus();
        var hasStopEffector = currentRoad.GetComponent<StopEffector>() != null;

        if (nextRoadStatus.statusType > NextRoadStatusType.ADJACENT) return hasStopEffector;
        
        var nextRoad = nextRoadStatus.statusType == NextRoadStatusType.ADJACENT
            ? nextRoadStatus.roadInfo.road
            : path[currentRoadIndex + 1].road;
        return hasStopEffector && nextRoad.GetComponent<StopEffector>() != null;
    }

    private IEnumerator WaitForStopEffector()
    {
        while (currentRoad.GetComponent<StopEffector>()?.remainTime > 0)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator LinearMove(GameObject[] points)
    {
        transform.position = Linear(points[0].transform.position, points[1].transform.position, t);
        t += speed;

        var dir = points[1].transform.position - points[0].transform.position;
        transform.right = dir;

        if (hasValidPath) currentRoad.UpdatePathRender(currentRoadRunningIndex, Mathf.Clamp(t * 1.1f, 0, 1));

        yield return new WaitForFixedUpdate();
    }

    private IEnumerator BezierMove(GameObject[] points)
    {
        var a = points[0].transform.position + (points[1].transform.position - points[0].transform.position).normalized * 0.15f;
        var b = points[2].transform.position + (points[1].transform.position - points[2].transform.position).normalized * 0.15f;
        var originalPosition = transform.position;
        switch (t)
        {
            case <= 0.2f:
                transform.position = Linear(points[0].transform.position, a, t * 5);
                break;
            case >= 0.8f:
                transform.position = Linear(b, points[2].transform.position, (t - 0.8f) * 5);
                break;
            default:
                transform.position = Bezier(a, points[1].transform.position, b, (t - 0.2f) * (10f / 6f));
                transform.right = transform.position - originalPosition;
                break;
        }

        t += (speed / 3) / Vector2.Distance(a, b) * 0.85f;

        if (hasValidPath) currentRoad.UpdatePathRender(currentRoadRunningIndex, Mathf.Clamp(t * 1.1f, 0, 1));

        yield return new WaitForFixedUpdate();
    }

    private bool InitCurrentRoad()
    {
        currentRoad = path[currentRoadIndex].road;
        currentRoadRunningIndex = path[currentRoadIndex].attachIndex;

        return currentRoadRunningIndex != -1;
    }

    private IEnumerator MoveAlongWayPoints()
    {
        var points = currentRoad.wayPointAry[currentRoadRunningIndex].points;
        if (points.Length <= 2)
        {
            while (t < 0.8f) yield return LinearMove(points);
            if (CheckStopEffector()) yield return WaitForStopEffector();
            while (t < 1) yield return LinearMove(points);
        }
        else
        {
            while (t < 0.8f) yield return BezierMove(points);
            if (CheckStopEffector()) yield return WaitForStopEffector();
            while (t < 1) yield return BezierMove(points);
        }
    }

    private bool PrepareNextRoad()
    {
        var nextRoadStatus = GetNextRoadStatus();
        var needNextMove = nextRoadStatus.statusType <= NextRoadStatusType.ADJACENT;

        switch (nextRoadStatus.statusType)
        {
            case NextRoadStatusType.ADJACENT:
                path.Add(nextRoadStatus.roadInfo);
                break;
            case NextRoadStatusType.DISCONNECTED:
                DestroyCar();
                break;
            case NextRoadStatusType.DESTINATION:
                ParkCar();
                break;
        }

        currentRoadIndex++;
        t = 0;

        return needNextMove;
    }

    private IEnumerator MoveRoutine()
    {
        ResetSpeed();

        var validRunningIndex = InitCurrentRoad();
        if (!validRunningIndex) yield return new WaitForFixedUpdate();

        yield return MoveAlongWayPoints();

        var needNextMove = PrepareNextRoad();
        if (needNextMove) StartCoroutine(MoveRoutine());
    }

    private PathInfo FindConnectedRoad(Cell targetCell)
    {
        Road nextRoad = null;
        var nextIndex = -1;

        foreach (var adjCell in targetCell.GetConnectedCellNotNull())
        {
            if (currentRoadIndex >= 1 && path[currentRoadIndex - 1].road == adjCell) continue;
            if (adjCell is not Road adjRoad) continue;

            var targetAdjIndex = targetCell is Road targetRoad 
                ? targetRoad.GetWayPointIndexTo(targetRoad.GetRelativePosition(adjRoad)) 
                : currentRoadRunningIndex;
            var adjRoadAdjIndex = adjRoad.GetWayPointIndexFrom(adjRoad.GetRelativePosition(targetCell));

            if (adjRoadAdjIndex != -1 && targetAdjIndex == currentRoadRunningIndex)
            {
                nextRoad = adjRoad;
                nextIndex = adjRoadAdjIndex;
                break;
            }
        }

        return new PathInfo(nextRoad, nextIndex);
    }

    private void ParkCar()
    {
        arrived = true;
        destinationPoint.ParkCar();
        Destroy(gameObject);
    }

    private void DestroyCar()
    {
        currentRoad = null;
        Destroy(gameObject);
    }

    public void ApplyTheme(Color color)
    {
        backgroundSprite.color = color;
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
        backgroundSprite = GetComponent<SpriteRenderer>();
    }
}