using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int rotation = 0;
    private bool isRotating = false;
    private Quaternion desireRot;

    public bool[] cellConnection = new bool[]
    {
        false, false, false, false
    };

    public Vector3Int cellPosition => GridController.grid.WorldToCell(transform.position);

    public Cell[] GetAdjacentCell()
    {
        var cellList = new List<Cell>();
        var offsetAry = new Vector2Int[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };

        for (int i = 0; i < offsetAry.Length; i++)
        {
            var cell = GridController.instance.GetCell(cellPosition + (Vector3Int)offsetAry[i]);
            cellList.Add(cell);
        }

        return cellList.ToArray();
    }

    public Cell[] GetAdjacentCellNotNull()
    {
        return Array.FindAll(GetAdjacentCell(), cell => cell != null);
    }

    public int GetRelativePosition(Cell target)
    {
        int direction = -1;
        var offsetAry = new Vector2Int[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };

        for (int i = 0; i < offsetAry.Length; i++)
        {
            var cell = GridController.instance.GetCell(cellPosition + (Vector3Int)offsetAry[i]);
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
            if (cell != null && isConnected(cell, index)) return cell;
            return null;
        }).ToArray();
    }

    public Cell[] GetConnectedCellNotNull()
    {
        return GetConnectedCell().Where(cell => cell != null).ToArray();
    }

    public bool isConnected(Cell target)
    {
        int direction = GetRelativePosition(target);
        return direction != -1 && isConnected(target, direction);
    }

    public bool isConnected(Cell target, int direction)
    {
        var targetDirection = direction;
        targetDirection += 2;
        targetDirection %= 4;

        return cellConnection[direction] && target.cellConnection[targetDirection];
    }

    public void Rotate()
    {
        var carAry = FindObjectsOfType<Car>();
        if (carAry.Any(car => car.currentRoad == this)) return;

        rotation += 1;
        rotation %= 4;

        StartRotate();
        RotateConnection();

        foreach (var point in GridController.instance.pointAry)
        {
            point.OnRotate();
        }

        foreach (var car in carAry)
        {
            car.OnRotate(this);
        }

        GridController.instance.OnRotate(this);
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

    private void StartRotate()
    {
        if (isRotating)
        {
            var step = (int)desireRot.eulerAngles.z / 90;
            step++;

            desireRot = Quaternion.Euler(0, 0, step * 90);
        }
        else
        {
            isRotating = true;
            var step = (int)transform.rotation.eulerAngles.z / 90;
            step++;

            desireRot = Quaternion.Euler(0, 0, step * 90);
        }
    }

    protected virtual void RotationUpdate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, desireRot, Time.deltaTime * 10);

        if (MathF.Abs(transform.rotation.eulerAngles.z - desireRot.eulerAngles.z) <= 0.1f)
        {
            transform.rotation = desireRot;
            isRotating = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!isRotating) return;
        RotationUpdate();
    }

    protected virtual void Awake()
    {
        rotation = (int)(transform.rotation.eulerAngles.z / 90);
        for (int i = 0; i < rotation; i++)
        {
            RotateConnection();
        }
    }
}