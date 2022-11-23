using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    private Grid grid;

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

    private void Start()
    {
        grid = GetComponent<Grid>();

        var pointAry = GetComponentsInChildren<Point>();
        var groupedPointAry = new Point[pointAry.Length / 2, 2];

        foreach (Point point in pointAry)
        {
            groupedPointAry[point.pointIndex, (int)point.pointType] = point;
        }

        for (int i = 0; i < pointAry.Length / 2; i++)
        {
            groupedPointAry[i, 0].otherPoint = groupedPointAry[i, 1];
            groupedPointAry[i, 1].otherPoint = groupedPointAry[i, 0];
        }
    }
}
