using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindResult
{
    public List<PathInfo> path;
    public Cell start;
    public Cell destination;

    public PathFindResult(List<PathInfo> path, Cell start, Cell destination)
    {
        this.path = path;
        this.start = start;
        this.destination = destination;
    }
}

public class GridController : MonoBehaviour
{
    private static GridController _instance;
    public static GridController instance => _instance ??= FindObjectOfType<GridController>();

    private static Grid _grid;
    public static Grid grid => _grid ??= instance.GetComponent<Grid>();

    public Point[] pointAry;

    private List<PathFindResult> pathFindResultList = new List<PathFindResult>();

    public Vector3 GetCellPosition(Vector3 worldPosition)
    {
        return grid.GetCellCenterWorld(grid.WorldToCell(worldPosition));
    }

    public Cell GetCell(Vector3 worldPosition)
    {
        return GetCell(grid.WorldToCell(worldPosition));
    }

    public Cell GetCell(Vector3Int cellPosition)
    {
        Cell cell = null;
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            var searchCell = grid.transform.GetChild(i).GetComponent<Cell>();
            if (searchCell == null) continue;

            if ((Vector2Int)cellPosition == (Vector2Int)searchCell.cellPosition)
            {
                cell = searchCell;
            }
        }

        return cell;
    }

    public void OnRotate()
    {
        pathFindResultList.Clear();
    }

    public List<PathInfo> RequestPath(int startRunningIndex, Cell start, Cell destination)
    {
        var pathFindResult = pathFindResultList.FirstOrDefault(res => res.start == start && res.destination == destination);
        return pathFindResult == null ? CalculatePath(startRunningIndex, start, destination) : pathFindResult.path;
    }

    private List<PathInfo> CalculatePath(int startRunningIndex, Cell start, Cell destination)
    {
        var shortestPath = new List<PathInfo>();
        var currentPath = new List<PathInfo>();

        PathFindRecursive(ref shortestPath, currentPath, startRunningIndex, start, destination);
        
        if (shortestPath.Count <= 0) return new List<PathInfo>();

        pathFindResultList.Add(new PathFindResult(shortestPath, start, destination));

        return shortestPath;
    }

    private void PathFindRecursive(ref List<PathInfo> shortestPath, List<PathInfo> path, int startRunningIndex, Cell start, Cell destination)
    {
        var connectedCellAry = start.GetConnectedCell();
        if (connectedCellAry.All(c => c == null)) return;

        foreach (Cell cell in connectedCellAry.Where(c => c != null))
        {
            if (cell == destination && path.Count != 0)
            {
                var prevRoad = start as Road;
                var index = prevRoad.GetWayPointIndexTo(prevRoad.GetRelativePosition(cell));

                if (index == path.Last().attachIndex && (path.Count < shortestPath.Count || shortestPath.Count == 0))
                {
                    shortestPath = path.ToList();
                    continue;
                }
            }

            if (cell is not Road adjRoad) continue;
            if (path.Exists(p => p.road == cell)) continue;

            var targetAdjIndex = start is Road startRoad
                ? startRoad.GetWayPointIndexTo(startRoad.GetRelativePosition(adjRoad))
                : 100;
            var adjRoadAdjIndex = adjRoad.GetWayPointIndexFrom(adjRoad.GetRelativePosition(start));

            if (adjRoadAdjIndex != -1 && (targetAdjIndex == startRunningIndex || targetAdjIndex == 100))
            {
                var newPath = path.ToList();
                newPath.Add(new PathInfo(adjRoad, adjRoadAdjIndex));
                PathFindRecursive(ref shortestPath, newPath, adjRoadAdjIndex, adjRoad, destination);
            }
        }
    }

    private void Awake()
    {
        pointAry = GetComponentsInChildren<Point>();
    }

    private void Start()
    {
        foreach (var startPoint in pointAry.Where(point => point.pointType == PointType.START))
        {
            var endPoint = pointAry.FirstOrDefault(point => point.pointType == PointType.END && point.pointTheme.name == startPoint.pointTheme.name);
            if (endPoint == null) continue;

            startPoint.otherPoint = endPoint;
            endPoint.otherPoint = startPoint;
        }
    }

    private void OnDestroy()
    {
        _instance = null;
        _grid = null;
    }
}
