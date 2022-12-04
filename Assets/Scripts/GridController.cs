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
    private Grid grid;
    private List<PathFindResult> pathFindResultList = new List<PathFindResult>();

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

    public List<PathInfo> RequestPath(Cell prev, Cell current, Cell destination)
    {
        var pathFindResult = pathFindResultList.FirstOrDefault(res => res.start == current && res.destination == destination);
        return pathFindResult == null ? CaculatePath(prev, current, destination) : pathFindResult.path;
    }

    private List<PathInfo> CaculatePath(Cell prev, Cell current, Cell destination)
    {
        var shortestPath = new List<PathInfo>();
        var currentPath = new List<PathInfo>();
        PathFindRecursive(ref shortestPath, currentPath, current, destination);
        if (shortestPath.Count <= 0) return shortestPath;

        for (int i = shortestPath.Count - 1; i >= 1; i--)
        {
            shortestPath[i].SetAttachIndex(shortestPath[i - 1].road);
        }
        shortestPath[0].SetAttachIndex(prev);

        pathFindResultList.Add(new PathFindResult(shortestPath, current, destination));

        return shortestPath;
    }

    private void PathFindRecursive(ref List<PathInfo> shortestPath, List<PathInfo> path, Cell current, Cell destination)
    {
        var connectedCellAry = current.GetConnectedCell();
        if (connectedCellAry.All(c => c == null)) return;

        foreach (Cell cell in connectedCellAry.Where(c => c != null))
        {
            if (cell == destination && path.Count != 0)
            {
                if (path.Count < shortestPath.Count || shortestPath.Count == 0)
                {
                    shortestPath = path.ToList();
                    continue;
                }
            }

            if (cell is not Road) continue;
            if (path.Exists(p => p.road == cell)) continue;

            var newPath = path.ToList();
            newPath.Add(new PathInfo(cell as Road, -1));
            PathFindRecursive(ref shortestPath, newPath, cell, destination);
        }
    }

    private void Start()
    {
        grid = GetComponent<Grid>();

        var pointAry = GetComponentsInChildren<Point>();
        foreach (var startPoint in pointAry.Where(point => point.pointType == PointType.START))
        {
            var endPoint = pointAry.FirstOrDefault(point => point.pointType == PointType.END && point.pointTheme.name == startPoint.pointTheme.name);
            if (endPoint == null) continue;

            startPoint.otherPoint = endPoint;
            endPoint.otherPoint = startPoint;
        }
    }
}
