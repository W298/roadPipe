using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effecter : MonoBehaviour
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

    public abstract IEnumerator Routine();
}

public class StopEffecter : Effecter
{
    private Road road;
    private bool active = false;

    public override IEnumerator Routine()
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

public class SlowEffecter : Effecter
{
    private Road road;
    private bool active = false;

    public override IEnumerator Routine()
    {
        road = GetComponent<Road>();
        active = true;

        GetComponent<SpriteRenderer>().sprite = road.slowSprite;

        if (duration < 0) yield break;
        yield return new WaitForSeconds(duration);
        active = false;

        GetComponent<SpriteRenderer>().sprite = road.originalSprite;

        road.carList.ForEach(car => car.ResetSpeed());
        Destroy(this);
    }

    private void Update()
    {
        if (!active) return;
        road.carList.ForEach(car => car.SlowDown());
    }
}
