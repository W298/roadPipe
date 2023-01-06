using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effector : MonoBehaviour
{
    public float duration = 5f;
    public Vector2 offset = Vector2.zero;
    public GameObject prefab;

    public void Init(float duration, Vector2 offset, GameObject prefab = null)
    {
        this.duration = duration;
        this.offset = offset;
        this.prefab = prefab;
    }

    public abstract IEnumerator Routine();
}

public class TimerEffector : Effector
{
    public override IEnumerator Routine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}

public class StopEffector : TimerEffector
{
    public float remainTime;

    public override IEnumerator Routine()
    {
        remainTime = duration;
        while (remainTime > 0)
        {
            yield return new WaitForSeconds(1f);
            remainTime -= 1f;
        }

        Destroy(this);
    }
}

public class SlowEffector : Effector
{
    private Road road;
    private bool active = false;

    public override IEnumerator Routine()
    {
        road = GetComponent<Road>();
        active = true;

        road.EnableSlowOverlay();

        if (duration < 0) yield break;
        yield return new WaitForSeconds(duration);
        active = false;

        road.DisableSlowOverlay();

        road.carList.ForEach(car => car.ResetSpeed());
        Destroy(this);
    }

    private void Update()
    {
        if (!active) return;
        road.carList.ForEach(car => car.SlowDown());
    }
}
