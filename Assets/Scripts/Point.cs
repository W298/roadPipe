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
    public float carSpawnDelay = 5;
    public float carSpawnBias = 1;
    public Theme pointTheme => GameManager.instance.GetTheme(pointThemeName);
    public Point otherPoint;
    public GameObject carPrefab;

    private int carCount = 0;

    private IEnumerator StartSpawnCar()
    {
        yield return new WaitForSeconds(carSpawnBias);
        StartCoroutine(SpawnCarLoop());
    }

    private IEnumerator SpawnCarLoop()
    {
        var car = Instantiate(carPrefab, transform.position, Quaternion.identity).GetComponent<Car>();
        car.ApplyTheme(pointTheme.color);
        car.name = carCount.ToString();
        carCount++;
        car.startPoint = this;
        car.destinationPoint = otherPoint;
        car.StartMove();

        yield return new WaitForSeconds(carSpawnDelay);
        StartCoroutine(SpawnCarLoop());
    }

    private void ApplyTheme()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = pointTheme.color;
    }

    private void Start()
    {
        ApplyTheme();
        if (pointType == PointType.START) StartCoroutine(StartSpawnCar());
    }
}
