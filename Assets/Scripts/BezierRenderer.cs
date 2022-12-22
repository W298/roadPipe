using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private LineRenderer maskRenderer;

    public Vector3 forward;
    public Vector3 start;
    public Vector3 control;
    public Vector3 end;
    public Vector3 backward;

    public float t = 1;

    public bool isLinear = false;
    public int resolution = 30;
    public Color color = Color.white;

    public Material material;
    public Material maskMaterial;

    public void Render()
    {
        SwitchSortingLayer();
        lineRenderer.startColor = lineRenderer.endColor = color;
        lineRenderer.positionCount = resolution + (isLinear ? 0 : 2);

        if (isLinear)
        {
            for (int i = 0; i < resolution; i++)
            {
                lineRenderer.SetPosition(i, Linear(start, end, (float)i / (resolution - 1)));
            }
        }
        else
        {
            lineRenderer.SetPosition(0, forward);
            lineRenderer.SetPosition(resolution + 1, backward);

            for (int i = 1; i < resolution + 1; i++)
            {
                lineRenderer.SetPosition(i, Bezier(start, control, end, (float)i / (resolution - 1)));
            }
        }

        maskRenderer.positionCount = Mathf.FloorToInt(lineRenderer.positionCount * t);
        for (int i = 0; i < maskRenderer.positionCount; i++)
        {
            maskRenderer.SetPosition(i, lineRenderer.GetPosition(i));
        }
    }

    private void SwitchSortingLayer()
    {
        if (t < 1)
        {
            lineRenderer.sortingLayerName = "PathLine Upper";
            maskRenderer.sortingLayerName = "PathLine Mask Upper";
        }
        else
        {
            lineRenderer.sortingLayerName = "PathLine";
            maskRenderer.sortingLayerName = "PathLine Mask";
        }
    }

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = material;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.2f;
        lineRenderer.sortingLayerName = "PathLine";

        maskRenderer = transform.parent.GetChild(transform.GetSiblingIndex() + transform.parent.childCount / 2).gameObject.AddComponent<LineRenderer>();
        maskRenderer.material = maskMaterial;
        maskRenderer.startWidth = maskRenderer.endWidth = 0.06f;
        maskRenderer.sortingLayerName = "PathLine Mask";
    }

    private static Vector2 Linear(Vector2 start, Vector2 end, float t)
    {
        return start + (end - start) * t;
    }

    private static Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        return (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
    }
}
