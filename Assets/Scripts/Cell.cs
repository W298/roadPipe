using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Cell : MonoBehaviour
{
    protected Grid grid;
    private GridController gridController;

    public bool[] cellConnection = new bool[]
    {
        false, false, false, false
    };

    public Vector3Int cellPosition => grid.WorldToCell(transform.position);

    public Cell[] GetAdjacentCell()
    {
        var cellList = new List<Cell>();
        var offsetAry = new Vector2Int[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };

        for (int i = 0; i < offsetAry.Length; i++)
        {
            var cell = gridController.GetCell(cellPosition + (Vector3Int)offsetAry[i]);
            cellList.Add(cell);
        }

        return cellList.ToArray();
    }

    public Cell[] GetAdjacentCellNotNull()
    {
        return Array.FindAll(GetAdjacentCell(), cell => cell != null);
    }

    public int GetAttachedIndex(Cell target)
    {
        int direction = -1;
        var offsetAry = new Vector2Int[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };

        for (int i = 0; i < offsetAry.Length; i++)
        {
            var cell = gridController.GetCell(cellPosition + (Vector3Int)offsetAry[i]);
            if (cell != null && cell == target)
            {
                direction = i;
                break;
            }
        }

        return direction;
    }

    public Cell[] GetConnectedCell()
    {
        return GetAdjacentCell().Select((cell, index) =>
        {
            if (cell != null && cellConnection[index] && isConnected(cell, index)) return cell;
            return null;
        }).ToArray();
    }

    public Cell[] GetConnectedCellNotNull()
    {
        return GetConnectedCell().Where(cell => cell != null).ToArray();
    }

    public bool isConnected(Cell target)
    {
        int direction = GetAttachedIndex(target);
        return direction != -1 && isConnected(target, direction);
    }

    public bool isConnected(Cell target, int direction)
    {
        direction += 2;
        direction %= 4;

        return target.cellConnection[direction];
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 0, 1), 90);
        RotateConnection();
        FindObjectOfType<Car>().OnRotate();
        gridController.OnRotate();
    }

    public void RotateConnection()
    {
        bool lastValue = cellConnection.Last();
        for (int i = cellConnection.Length - 2; i >= 0; i--)
        {
            cellConnection[i + 1] = cellConnection[i];
        }
        cellConnection[0] = lastValue;
    }

    protected void Start()
    {
        grid = transform.parent.GetComponent<Grid>();
        gridController = transform.parent.GetComponent<GridController>();
    }
}