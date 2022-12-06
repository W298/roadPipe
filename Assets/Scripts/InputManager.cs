using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    private GridController gridController;

    private void Awake()
    {
        gridController = FindObjectOfType<GridController>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = gridController.GetCell(worldPos);
            if (cell != null && cell is Road)
            {
                cell.Rotate();
            }
        }
    }
}
