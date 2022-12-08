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

    public int carCount = 5;
    public int arrivedCarCount = 0;
    public List<Sprite> pointSpriteList;

    private SpriteRenderer spriteRenderer;

    public void ParkCar()
    {
        arrivedCarCount++;
    }

    public void OnRotate()
    {
        ApplySprite();
    }

    private IEnumerator StartSpawnCar()
    {
        yield return new WaitForSeconds(carSpawnBias);
        StartCoroutine(SpawnCarLoop());
    }

    private IEnumerator SpawnCarLoop()
    {
        var car = Instantiate(carPrefab, transform.position, Quaternion.identity).GetComponent<Car>();
        car.ApplyTheme(pointTheme.color);
        car.startPoint = this;
        car.destinationPoint = otherPoint;
        car.StartMove();

        carCount--;

        if (carCount < 0) yield break;
        yield return new WaitForSeconds(carSpawnDelay);
        StartCoroutine(SpawnCarLoop());
    }

    private void ApplyTheme()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = pointTheme.color;
    }

    private void ApplySprite()
    {
        var connectedCell = GetConnectedCell();
        int attachedIndex = 4;
        for (int i = 0; i < connectedCell.Length; i++)
        {
            if (connectedCell[i] is Road)
            {
                attachedIndex = i;
                break;
            }
        }

        spriteRenderer.sprite = pointSpriteList[attachedIndex];
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyTheme();
        ApplySprite();
        if (pointType == PointType.START) StartCoroutine(StartSpawnCar());
    }
}
