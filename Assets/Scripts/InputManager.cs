using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEditor.Progress;

public enum InputMode
{
    ROTATE,
    SLOW,
    STOP
}

public class InputManager : MonoBehaviour
{
    public GameObject selectorPrefab;
    public GameObject stopCursorPrefab;
    public GameObject slowCursorPrefab;

    private GameObject _selector = null;

    private GameObject selector
    {
        get
        {
            if (_selector == null)
            {
                _selector = Instantiate(selectorPrefab, Vector3.zero, Quaternion.identity);
            }
            return _selector;
        }
    }

    private GameObject _stopCursor = null;
    private GameObject stopCursor
    {
        get
        {
            if (_stopCursor == null)
            {
                _stopCursor = Instantiate(stopCursorPrefab, Vector3.zero, Quaternion.identity);
                _stopCursor.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
            return _stopCursor;
        }
    }

    private GameObject _slowCursor = null;
    private GameObject slowCursor
    {
        get
        {
            if (_slowCursor == null)
            {
                _slowCursor = Instantiate(slowCursorPrefab, Vector3.zero, Quaternion.identity);

                foreach (var sprite in _slowCursor.GetComponentsInChildren<SpriteRenderer>())
                {
                    sprite.color = new Color(1, 1, 1, 0.5f);
                }
            }
            return _slowCursor;
        }
    }

    private InputMode inputMode;

    private void ChangeInputMode()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputMode = InputMode.SLOW;
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            inputMode = InputMode.STOP;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            inputMode = InputMode.ROTATE;
        }
    }

    private Tuple<Road, Road, Vector3, Quaternion> SetCursor(Vector3 worldPos)
    {
        selector.SetActive(false);
        stopCursor.SetActive(false);
        slowCursor.SetActive(false);

        var spawnPos = GridController.instance.GetCellPosition(worldPos);
        switch (inputMode)
        {
            case InputMode.SLOW:
                selector.SetActive(true);
                selector.transform.position = spawnPos;
                slowCursor.SetActive(true);
                slowCursor.transform.position = spawnPos;
                break;
            case InputMode.STOP:
                stopCursor.SetActive(true);

                var vX = (float)Math.Round(worldPos.x * 2, MidpointRounding.AwayFromZero) / 2;
                var vY = (float)Math.Round(worldPos.y * 2, MidpointRounding.AwayFromZero) / 2;

                var roundedPos = new Vector3(vX, vY, 0);

                var xOdd = (int)(vX / 0.5f) % 2 != 0;
                var yOdd = (int)(vY / 0.5f) % 2 != 0;

                if (xOdd == yOdd) break;

                var a = roundedPos + (xOdd ? new Vector3(0, 0.5f, 0) : new Vector3(0.5f, 0, 0));
                var b = roundedPos + (xOdd ? new Vector3(0, -0.5f, 0) : new Vector3(-0.5f, 0, 0));

                var aRoad = GridController.instance.GetCell(a) is Road ? GridController.instance.GetCell(a) as Road : null;
                var bRoad = GridController.instance.GetCell(b) is Road ? GridController.instance.GetCell(b) as Road : null;

                if (aRoad == null && bRoad == null) break;

                var cond = (aRoad != null && aRoad.cellConnection[xOdd ? 3 : 2]) || (bRoad != null && bRoad.cellConnection[xOdd ? 1 : 0]);
                if (!cond) break;

                stopCursor.transform.rotation = xOdd ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
                stopCursor.transform.position = roundedPos;
                return new Tuple<Road, Road, Vector3, Quaternion>(aRoad, bRoad, roundedPos, stopCursor.transform.rotation);
            default:
                selector.SetActive(true);
                selector.transform.position = spawnPos;
                break;
        }

        return new Tuple<Road, Road, Vector3, Quaternion>(null, null, Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        ChangeInputMode();
        var result = SetCursor(worldPos);

        if (Input.GetMouseButtonDown(0))
        {
            var cell = GridController.instance.GetCell(worldPos);
            switch (inputMode)
            {
                case InputMode.ROTATE:
                    if (cell == null || cell is not Road) break;
                    cell.Rotate();
                    break;
                case InputMode.SLOW:
                    if (cell == null || cell is not Road) break;
                    
                    if (GameManager.instance.inventoryManager.GetCount(ItemType.SLOW) == 0) break;
                    GameManager.instance.inventoryManager.UseItem(ItemType.SLOW);
                    
                    var slowEffector = cell.gameObject.AddComponent<SlowEffector>();
                    slowEffector.Init(-1f, Vector2.zero);
                    StartCoroutine(slowEffector.Routine());
                    break;
                case InputMode.STOP:
                    if (GameManager.instance.inventoryManager.GetCount(ItemType.STOP) == 0) break;
                    GameManager.instance.inventoryManager.UseItem(ItemType.STOP);

                    var blockPrefab = Instantiate(stopCursorPrefab, result.Item3, result.Item4);
                    var timerEffector = blockPrefab.AddComponent<TimerEffector>();
                    timerEffector.Init(5f, Vector2.zero);
                    StartCoroutine(timerEffector.Routine());

                    var effectRoadList = new Road[] { result.Item1, result.Item2 };
                    foreach (var road in effectRoadList)
                    {
                        if (road == null) continue;
                        var stopEffector = road.AddComponent<StopEffector>();
                        stopEffector.Init(5f, Vector2.zero);
                        StartCoroutine(stopEffector.Routine());
                    }

                    break;
            }
        }
    }
}
