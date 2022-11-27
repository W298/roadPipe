using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    protected Grid grid;
    private GridController gridController;

    public Vector3Int cellPosition => grid.WorldToCell(transform.position);

    public Cell[] GetAdjacentCell()
    {
        var cellList = new List<Cell>();
        var offsetAry = new Vector2Int[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        for (int i = 0; i < offsetAry.Length; i++)
        {
            var cell = gridController.GetCell(cellPosition + (Vector3Int)offsetAry[i]);
            if (cell != null) cellList.Add(cell);
        }

        return cellList.ToArray();
    }

    public ConnectionResult isConnected(Cell target)
    {
        if (this is Point && target is Point)
        {
            return new ConnectionResult(-1, -1, false);
        }

        if (this is Point && target is Road)
        {
            return ((Point)this).isConnected((Road)target);
        }

        if (this is Road && target is Point)
        {
            return ((Point)target).isConnected((Road)this);
        }

        return ((Road)this).isConnected((Road)target);
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 0, 1), 90);
    }

    protected void Start()
    {
        grid = transform.parent.GetComponent<Grid>();
        gridController = transform.parent.GetComponent<GridController>();
    }
}