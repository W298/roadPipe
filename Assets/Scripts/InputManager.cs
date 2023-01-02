using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public enum InputMode
{
    ROTATE,
    SLOW,
    STOP
}

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager instance => _instance ??= FindObjectOfType<InputManager>();

    public GameObject selectorPrefab;
    public GameObject stopCursorPrefab;
    public GameObject slowCursorPrefab;

    private AudioSource audioSource;
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
    private bool isCursorDisabled = false;

    public InputAction slowItemAction;
    public InputAction stopItemAction;
    public InputAction rotateAction;
    public InputAction pauseAction;

    private Tuple<Road, Road, Vector3, Quaternion> SetCursor(Vector3 worldPos)
    {
        selector.SetActive(false);
        stopCursor.SetActive(false);
        slowCursor.SetActive(false);

        if (isCursorDisabled)
        {
            return new Tuple<Road, Road, Vector3, Quaternion>(null, null, Vector3.zero, Quaternion.identity);
        }

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

        var result = SetCursor(worldPos);

        if (Input.GetMouseButtonDown(0) && !isCursorDisabled)
        {
            audioSource.Play();
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

    private void Awake()
    {
        var stopKey = stopItemAction.bindings[0].path[11..].ToUpper();
        var slowKey = slowItemAction.bindings[0].path[11..].ToUpper();
        var rotateKey = rotateAction.bindings[0].path[11..].ToUpper();
        
        PlayerUI.instance.UpdateKeyUI(stopKey, slowKey, rotateKey);
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        slowItemAction.Enable();
        stopItemAction.Enable();
        rotateAction.Enable();
        pauseAction.Enable();

        slowItemAction.performed += context => inputMode = InputMode.SLOW;
        stopItemAction.performed += context => inputMode = InputMode.STOP;
        rotateAction.performed += context => inputMode = InputMode.ROTATE;
        pauseAction.performed += context => PlayerUI.instance.TogglePauseMenu();
    }

    private void OnDestroy()
    {
        _instance = null;

        slowItemAction.ChangeBinding(0).Erase();
        stopItemAction.ChangeBinding(0).Erase();
        rotateAction.ChangeBinding(0).Erase();
        pauseAction.ChangeBinding(0).Erase();
    }

    public void EnableCursor()
    {
        isCursorDisabled = false;
        switch (inputMode)
        {
            case InputMode.ROTATE:
                selector?.SetActive(true);
                break;
            case InputMode.SLOW:
                slowCursor?.SetActive(true);
                break;
            case InputMode.STOP:
                stopCursor?.SetActive(true);
                break;
        }
    }

    public void DisableCursor()
    {
        isCursorDisabled = true;
        selector.SetActive(false);
        slowCursor.SetActive(false);
        stopCursor.SetActive(false);
    }
}
