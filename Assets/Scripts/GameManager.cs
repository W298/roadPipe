using StageSelectorUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarArriveInfo
{
    public Point startPoint;
    public Car car;
    public bool isEnd = false;

    public CarArriveInfo(Point startPoint, Car car)
    {
        this.startPoint = startPoint;
        this.car = car;
    }
}

[Serializable]
public class ItemDefaultValue
{
    public ItemType type;
    public bool allow = true;
    public int value = 5;
}

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager instance => _instance ??= FindObjectOfType<GameManager>();

    private int currentScore = 0;

    public ItemDefaultValue[] defaultValue;

    public ThemePalette themePalette;
    public InventoryManager inventoryManager;
    public List<CarArriveInfo> lastCarList = new List<CarArriveInfo>();

    public bool GetItemAllow(ItemType type)
    {
        return defaultValue.Any(v => v.type == type && v.allow);
    }

    public Theme GetTheme(ThemeName name)
    {
        return themePalette.palette.FirstOrDefault(theme => theme.name == name);
    }

    public void OnParkCar()
    {
        var goalCarCount = 0;
        var currentCarCount = 0;
        foreach (var point in GridController.instance.pointAry.Where(point => point.pointType == PointType.END))
        {
            goalCarCount += point.carCount;
            currentCarCount += point.arrivedCarCount;
        }

        var score = (float)currentCarCount / ((float)goalCarCount / 5f);
        currentScore = Mathf.FloorToInt(score);
        ScoreIndicator.instance.Render(currentScore);
    }

    public void EndCar(Car car)
    {
        var target = lastCarList.Find(info => info.car == car);
        if (target == null) return;

        target.isEnd = true;
        if (lastCarList.Count == GridController.instance.pointAry.Length / 2 && lastCarList.All(info => info.isEnd)) EndGame();
    }

    public void SaveScore()
    {
        var str = SceneManager.GetActiveScene().name;
        StageClearDataManager.instance.SaveScore(str[0].ToString(), int.Parse(str[1..].TrimStart('0')) - 1, currentScore);
    }

    private void EndGame()
    {
        EndGameController.instance.gameObject.SetActive(true);
    }

    private void Awake()
    {
        inventoryManager = new InventoryManager(defaultValue);
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
