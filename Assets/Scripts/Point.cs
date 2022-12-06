using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PointType
{
    START = 0,
    END = 1
}

public class Point : Cell
{
    public PointType pointType;
    public ThemeName pointThemeName;
    public Theme pointTheme => GameManager.instance.GetTheme(pointThemeName);
    public Point otherPoint;
    public GameObject carPrefab;

    private int carCount = 0;

    private IEnumerator SpawnCar()
    {
        yield return new WaitForSeconds(1f);
        var car = Instantiate(carPrefab, transform.position, Quaternion.identity).GetComponent<Car>();
        car.name = carCount.ToString();
        carCount++;
        car.startPoint = this;
        car.destinationPoint = otherPoint;
        car.StartMove();

        yield return new WaitForSeconds(2f);
        StartCoroutine(SpawnCar());
    }

    private void ApplyTheme()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = pointTheme.color;
    }

    private void Start()
    {
        ApplyTheme();
        if (pointType == PointType.START) StartCoroutine(SpawnCar());
    }
}
