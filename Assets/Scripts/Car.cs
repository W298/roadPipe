using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

public class PathVisualizerInfo
{
    public Car owner;
    public PathInfo pathInfo;
    public GameObject mask;

    public PathVisualizerInfo(Car owner, PathInfo pathInfo, GameObject mask)
    {
        this.owner = owner;
        this.pathInfo = pathInfo;
        this.mask = mask;
    }
}

public class Car : MonoBehaviour
{
    [Serialize] public List<PathInfo> path = new List<PathInfo>();
    public int currentRoadIndex = 0;
    public int currentRoadRunningIndex = -1;
    public bool hasValidPath = false;
    public bool arrived = false;
    public bool crashed = false;

    public Point startPoint;
    public Point destinationPoint;

    public GameObject crashParticleSystem;

    [SerializeField]
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
    private GameObject skullUI;
    private Canvas canvas;

    public GameObject maskPrefab;
    private List<PathVisualizerInfo> pathVisualizerList = new List<PathVisualizerInfo>();

    public void ClearPathVisualizer()
    {
        foreach (var o in pathVisualizerList)
        {
            o.pathInfo.road.maskGameObjectList.Remove(o.mask);
            Destroy(o.mask);
        }
        pathVisualizerList.Clear();
    }

    public void SpawnPathVisualizer()
    {
        foreach (var pathInfo in path.Where((info, index) => index >= currentRoadIndex))
        {
            var targetRoad = pathInfo.road;
            var isTargetCurrentRoad = targetRoad == currentRoad;
            var isAttachIndexEqual = pathInfo.attachIndex == currentRoadRunningIndex;

            targetRoad.ApplyDashColor(pathInfo.attachIndex, backgroundSprite.color);

            var mask = Instantiate(maskPrefab, targetRoad.transform.position, targetRoad.transform.rotation);
            mask.GetComponent<SpriteMask>().sprite = targetRoad.GetMask(isTargetCurrentRoad && isAttachIndexEqual ? t : 0, pathInfo.attachIndex);

            targetRoad.maskGameObjectList.Add(mask);
            pathVisualizerList.Add(new PathVisualizerInfo(this, pathInfo, mask));
        }
    }

    public void UpdatePathVisualizer()
    {
        if (!hasValidPath) return;
        var info = pathVisualizerList.Find(bunch => bunch.pathInfo.road == currentRoad && bunch.owner == this);
        
        if (info == null) return;
        info.mask.GetComponent<SpriteMask>().sprite = currentRoad.GetMask(t, currentRoadRunningIndex);
    }

    public void RemovePastPathVisualizer()
    {
        var info = pathVisualizerList.Find(bunch => bunch.pathInfo.road == currentRoad && bunch.owner == this);

        if (info == null) return;

        currentRoad.maskGameObjectList.Remove(info.mask);
        Destroy(info.mask);
        pathVisualizerList.Remove(info);
    }

    public bool PathFind(int startRunningIndex, Cell start, Cell destination)
    {
        ClearPathVisualizer();

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

        SpawnPathVisualizer();

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

    public void OnRotate(Cell target)
    {
        if (!NeedPathFind()) return;
        if (hasValidPath)
        {
            var index = path.FindIndex(info => info.road == target);
            if (index == -1) return;
        }

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
        return !arrived && !destinationPoint.isConnected(currentRoad) && !crashed;
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
        yield return LinearMove(points.Select(point => point.transform.position).ToArray());
    }

    private IEnumerator LinearMove(Vector3[] vectors)
    {
        transform.position = Linear(vectors[0], vectors[1], t);
        t += speed;

        var dir = vectors[1] - vectors[0];
        transform.right = dir;

        UpdatePathVisualizer();

        yield return new WaitForFixedUpdate();
    }

    private IEnumerator BezierMove(GameObject[] points)
    {
        yield return BezierMove(points.Select(point => point.transform.position).ToArray());
    }

    private IEnumerator BezierMove(Vector3[] vectors)
    {
        var a = vectors[0] + (vectors[1] - vectors[0]).normalized * 0.15f;
        var b = vectors[2] + (vectors[1] - vectors[2]).normalized * 0.15f;
        var originalPosition = transform.position;
        switch (t)
        {
            case <= 0.2f:
                transform.position = Linear(vectors[0], a, t * 5);
                break;
            case >= 0.8f:
                transform.position = Linear(b, vectors[2], (t - 0.8f) * 5);
                break;
            default:
                transform.position = Bezier(a, vectors[1], b, (t - 0.2f) * (10f / 6f));
                transform.right = transform.position - originalPosition;
                break;
        }

        t += (speed / 3) / Vector2.Distance(a, b) * 0.85f;

        UpdatePathVisualizer();

        yield return new WaitForFixedUpdate();
    }

    private IEnumerator UTurn(GameObject[] points)
    {
        yield return UTurn(points.Select(point => point.transform.position).ToArray());
    }

    private IEnumerator UTurn(Vector3[] vectors)
    {
        var originalPosition = transform.position;
        switch (t)
        {
            case <= 0.32f:
                transform.position = Linear(vectors[0], vectors[1], t * 3.125f);
                break;
            case >= 0.68f:
                transform.position = Linear(vectors[3], vectors[4], (t - 0.68f) * 3.125f);
                break;
            default:
                transform.position = Bezier(vectors[1], vectors[2], vectors[3], (t - 0.32f) * (10f / 3.6f));
                break;
        }

        transform.right = transform.position - originalPosition;

        t += speed / 2;

        UpdatePathVisualizer();

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
        
        if (points.Length == 2)
        {
            while (t < 0.8f) yield return LinearMove(points);
            if (CheckStopEffector()) yield return WaitForStopEffector();
            while (t < 1) yield return LinearMove(points);
        }
        else if (points.Length == 3)
        {
            while (t < 0.8f) yield return BezierMove(points);
            if (CheckStopEffector()) yield return WaitForStopEffector();
            while (t < 1) yield return BezierMove(points);
        }
        else
        {
            while (t < 0.8f) yield return UTurn(points);
            if (CheckStopEffector()) yield return WaitForStopEffector();
            while (t < 1) yield return UTurn(points);
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
                StartCoroutine(CrashCar());
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

        RemovePastPathVisualizer();

        var needNextMove = PrepareNextRoad();
        if (needNextMove) StartCoroutine(MoveRoutine());
    }

    private PathInfo FindConnectedRoad(Cell targetCell)
    {
        Road nextRoad = null;
        var nextIndex = -1;

        foreach (var adjCell in targetCell.GetConnectedCellNotNull())
        {
            if (adjCell is not Road adjRoad) continue;

            var targetAdjIndex = targetCell is Road targetRoad 
                ? targetRoad.GetWayPointIndexTo(targetRoad.GetRelativePosition(adjRoad)) 
                : currentRoadRunningIndex;
            var adjRoadAdjIndex = adjRoad.GetWayPointIndexFrom(adjRoad.GetRelativePosition(targetCell));
            var isCellUTurn = targetCell.cellConnection.Count(b => b) == 1;

            if (currentRoadIndex >= 1 && path[currentRoadIndex - 1].road == adjCell && (!isCellUTurn || path[currentRoadIndex - 1].attachIndex == adjRoadAdjIndex)) continue;

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

        GameManager.instance.EndCar(this);
        Destroy(gameObject);
    }

    private IEnumerator CrashCar()
    {
        crashed = true;

        GetComponent<AudioSource>().Play();

        var crashParticleGameObject = Instantiate(crashParticleSystem, Vector3.zero, Quaternion.Euler(-90, 0, 0));
        crashParticleGameObject.transform.parent = transform;
        crashParticleGameObject.transform.localPosition = new Vector3(0, 0, 60);
        crashParticleGameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        var duration = crashParticleGameObject.GetComponent<ParticleSystem>().main.duration;
        
        var lastPoints = currentRoad.wayPointAry[currentRoadRunningIndex].points;
        var lastDirection = lastPoints[^1].transform.position - lastPoints[^2].transform.position;
        
        var vectors = new Vector3[]
        {
            lastPoints[^1].transform.position,
            lastPoints[^1].transform.position + lastDirection * 0.2f
        };

        t = 0;
        speed = 0.04f;
        while (t < 1)
        {
            yield return LinearMove(vectors);
        }

        currentRoad = null;
        skullUI.SetActive(true);

        yield return new WaitForSeconds(duration + 0.5f);

        GameManager.instance.EndCar(this);
        Destroy(gameObject);
    }

    public void ApplyTheme(Color color)
    {
        backgroundSprite.color = color;
        skullUI.GetComponent<Image>().color = color;
    }

    private static Vector2 Linear(Vector2 start, Vector2 end, float t)
    {
        return start + (end - start) * t;
    }

    private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        return (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
    }

    private void Update()
    {
        canvas.transform.rotation = Quaternion.identity;
    }

    private void Awake()
    {
        backgroundSprite = GetComponent<SpriteRenderer>();
        canvas = GetComponentInChildren<Canvas>();
        skullUI = canvas.transform.GetChild(0).gameObject;
    }

    private void OnDestroy()
    {
        ClearPathVisualizer();
    }
}