using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum PointType
{
    START = 0,
    END = 1
}

public class Point : Cell
{
    public PointType pointType;
    public int pointIndex;
    public Point otherPoint;

    public Vector2[] attachPoint;

    public GameObject carPrefab;

    public ConnectionResult isConnected(Road road)
    {
		foreach (var way in road.wayPoint)
		{
			Vector2[] origin = new Vector2[]
			{
				way.points.First().transform.position,
				way.points.Last().transform.position
			};

			for (int i = 0; i < attachPoint.Length; i++)
			{
				for (int j = 0; j < origin.Length; j++)
				{
					if (Vector2.Distance(attachPoint[i], origin[j]) <= 0.001f)
					{
						return new ConnectionResult(i / 2, j, true);
					}
				}
			}
		}

		return new ConnectionResult(-1, -1, false);
    }

    private IEnumerator SpawnCar()
    {
        yield return new WaitForSeconds(1f);
        var car = Instantiate(carPrefab, transform.position, Quaternion.identity).GetComponent<Car>();
        car.start = this;
        car.destination = otherPoint;
        car.PathFind();
    }

    private void SetAttachPoint()
    {
        var offsetList = new Vector2[]
        {
            new(0.5f, -0.095f),
            new(0.5f, 0.095f),
            new(0.095f, 0.5f),
            new(-0.095f, 0.5f),
            new(-0.5f, 0.095f),
            new(-0.5f, -0.095f),
            new(-0.095f, -0.5f),
            new(0.095f, -0.5f),
        };

        attachPoint = offsetList.Select(offset => (Vector2)transform.position + offset).ToArray();
    }

    protected new void Start()
    {
        base.Start();
        SetAttachPoint();
        if (pointType == PointType.START) StartCoroutine(SpawnCar());
    }
}
