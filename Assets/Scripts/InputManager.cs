using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public enum InputMode
{
    ROTATE,
    SINGLE_ITEM,
    RANGE_ITEM
}

public abstract class Item : MonoBehaviour
{
    public float duration = 5f;
    public Vector2 offset = Vector2.zero;
    public GameObject prefab;

    public void Init(float duration, Vector2 offset, GameObject prefab)
    {
        this.duration = duration;
        this.offset = offset;
        this.prefab = prefab;
    }
}

public class StopItem : Item
{
    private Road road;
    private bool active = false;

    public IEnumerator Routine()
    {
        road = GetComponent<Road>();

        var prefabObject = Instantiate(prefab, transform.position + (Vector3)offset, Quaternion.identity);

        active = true;
        
        if (duration < 0) yield break;
        yield return new WaitForSeconds(duration);
        active = false;

        road.carList.ForEach(car => car.ResetSpeed());
        Destroy(prefabObject);
        Destroy(this);
    }

    private void Update()
    {
        if (!active) return;
        road.carList.ForEach(car => car.PauseMove());
    }
}

public class SlowItem : Item
{
    private Road road;
    private bool active = false;

    public IEnumerator Routine()
    {
        road = GetComponent<Road>();

        var prefabObject = Instantiate(prefab, transform.position + (Vector3)offset, Quaternion.identity);

        active = true;

        if (duration < 0) yield break;
        yield return new WaitForSeconds(duration);
        active = false;

        road.carList.ForEach(car => car.ResetSpeed());
        Destroy(prefabObject);
        Destroy(this);
    }

    private void Update()
    {
        if (!active) return;
        road.carList.ForEach(car => car.SlowDown());
    }
}

public class InputManager : MonoBehaviour
{
    public GameObject selectorPrefab;
    public GameObject stopPrefab;

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

    private GameObject _stop = null;
    private GameObject stop
    {
        get
        {
            if (_stop == null)
            {
                _stop = Instantiate(stopPrefab, Vector3.zero, Quaternion.identity);
                _stop.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
            return _stop;
        }
    }

    private InputMode inputMode;

    private void Update()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.S))
        {
            inputMode = InputMode.SINGLE_ITEM;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            inputMode = InputMode.ROTATE;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var cell = GridController.instance.GetCell(worldPos);
            if (cell != null && cell is Road)
            {
                switch (inputMode)
                {
                    case InputMode.ROTATE:
                        cell.Rotate();
                        break;
                    case InputMode.SINGLE_ITEM:
                        var item = cell.gameObject.AddComponent<SlowItem>();
                        item.Init(-1f, new Vector2(0, 0.5f), stopPrefab);
                        StartCoroutine(item.Routine());
                        break;
                }
            }
        }

        selector.SetActive(false);
        stop.SetActive(false);

        var spawnPos = GridController.instance.GetCellPosition(worldPos);
        switch (inputMode)
        {
            case InputMode.ROTATE:
                selector.SetActive(true);
                selector.transform.position = spawnPos;
                break;
            case InputMode.SINGLE_ITEM:
                stop.SetActive(true);
                stop.transform.position = spawnPos + new Vector3(0, 0.5f, 0);
                break;
        }
        
    }
}
