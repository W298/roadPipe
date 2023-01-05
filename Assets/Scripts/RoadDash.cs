using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadDash : MonoBehaviour
{
    public void Render(float t)
    {
        var vt = (float)(Math.Truncate(t * 10) / 10);
    }

    private void Start()
    {
        Render(0.5f);
    }
}
