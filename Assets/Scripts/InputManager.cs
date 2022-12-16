using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private void HandleInput(Cell cell)
    {
        switch (inputMode)
        {
            case InputMode.ROTATE:
                cell.Rotate();
                break;
            case InputMode.SLOW:
                if (GameManager.instance.inventoryManager.GetCount(ItemType.SLOW) == 0) break;
                GameManager.instance.inventoryManager.UseItem(ItemType.SLOW);
                var slowEffecter = cell.gameObject.AddComponent<SlowEffecter>();
                slowEffecter.Init(-1f, Vector2.zero, slowCursorPrefab);
                StartCoroutine(slowEffecter.Routine());
                break;
            case InputMode.STOP:
                if (GameManager.instance.inventoryManager.GetCount(ItemType.STOP) == 0) break;
                GameManager.instance.inventoryManager.UseItem(ItemType.STOP);
                var stopEffecter = cell.gameObject.AddComponent<StopEffecter>();
                stopEffecter.Init(5f, Vector2.zero, stopCursorPrefab);
                StartCoroutine(stopEffecter.Routine());
                break;
        }
    }

    private void SetCursor(Vector3 worldPos)
    {
        selector.SetActive(false);
        stopCursor.SetActive(false);
        slowCursor.SetActive(false);

        var spawnPos = GridController.instance.GetCellPosition(worldPos);
        switch (inputMode)
        {
            case InputMode.ROTATE:
                selector.SetActive(true);
                selector.transform.position = spawnPos;
                break;
            case InputMode.SLOW:
                slowCursor.SetActive(true);
                slowCursor.transform.position = spawnPos;
                break;
            case InputMode.STOP:
                stopCursor.SetActive(true);
                stopCursor.transform.position = spawnPos;
                break;
        }
    }

    private void Update()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        ChangeInputMode();

        if (Input.GetMouseButtonDown(0))
        {
            var cell = GridController.instance.GetCell(worldPos);
            if (cell != null && cell is Road) HandleInput(cell);
        }

        SetCursor(worldPos);
    }
}
