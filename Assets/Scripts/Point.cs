using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    public float remainDelay = 0;
    public float remainBias = 0;
    public Theme pointTheme => GameManager.instance.GetTheme(pointThemeName);
    public Point otherPoint;
    public GameObject carPrefab;

    public int carCount = 5;
    public int arrivedCarCount = 0;
    public List<Sprite> pointSpriteList;
    public Sprite rotatedShadowSprite;

    public bool isInfinite = false;
    public AudioClip sfxClip;

    private SpriteRenderer spriteRenderer;
    private Image biasBackground;
    private Text spawnDelayText;
    private Text spawnBiasText;
    private List<GameObject> carDummyList = new List<GameObject>();

    private AudioSource audioSource;

    public void ParkCar()
    {
        arrivedCarCount++;
        PlaySound(0.6f);
        UpdateCarDummy();
        GameManager.instance.OnParkCar();
    }

    public void OnRotate()
    {
        ApplySprite();
    }

    private void PlaySound(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    private IEnumerator StartSpawnCar()
    {
        remainBias = carSpawnBias;
        while (remainBias > 0)
        {
            UpdateSpawnBiasUI();
            yield return new WaitForSeconds(1f);
            remainBias--;
        }

        biasBackground.gameObject.SetActive(false);
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
        PlaySound(1.2f);
        UpdateCarDummy();

        if (carCount <= 0)
        {
            if (isInfinite)
            {
                carCount = 6;
                ResetCarDummy();
            }
            else
            {
                GameManager.instance.lastCarList.Add(new CarArriveInfo(this, car));
                remainDelay = 0;
                UpdateSpawnDelayUI();
                yield break;
            }
        }

        remainDelay = carSpawnDelay;
        while (remainDelay > 0)
        {
            UpdateSpawnDelayUI();
            yield return new WaitForSeconds(1f);
            remainDelay--;
        }
        
        StartCoroutine(SpawnCarLoop());
    }

    private void ApplyTheme()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = pointTheme.color;
        for (int i = 4; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().color = pointTheme.color;
        }
        biasBackground.color = pointTheme.color;
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

        if (attachedIndex >= 2 && attachedIndex != 4)
        {
            transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 180);
            spawnDelayText.alignment = TextAnchor.MiddleLeft;
            spawnDelayText.rectTransform.anchoredPosition = new Vector3(0, 0.2f, 0);
            transform.GetChild(3).localPosition = new Vector3(-0.314f, -0.222f, 0);
            transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = rotatedShadowSprite;
        }

        spriteRenderer.sprite = pointSpriteList[attachedIndex];
    }

    private void UpdateSpawnDelayUI()
    {
        spawnDelayText.text = remainDelay.ToString();
    }

    private void UpdateSpawnBiasUI()
    {
        spawnBiasText.text = remainBias.ToString();
    }

    private void ResetCarDummy()
    {
        foreach (var car in carDummyList)
        {
            ShowDummy(car);
        }
    }

    private void UpdateCarDummy()
    {
        if (pointType == PointType.START)
        {
            if (carCount < 0) return;
            for (int i = 0; i < carDummyList.Count - carCount; i++)
            {
                HideDummy(carDummyList[i]);
            }
            return;
        }

        for (int i = 0; i < arrivedCarCount; i++)
        {
            ShowDummy(carDummyList[i]);
        }
    }

    private void ShowDummy(GameObject dummy)
    {
        dummy.GetComponent<SpriteRenderer>().color = pointTheme.color;
        dummy.transform.GetChild(0).gameObject.SetActive(true);
        dummy.transform.GetChild(1).gameObject.SetActive(true);
    }

    private void HideDummy(GameObject dummy)
    {
        dummy.GetComponent<SpriteRenderer>().color = new Color(0.1921569f, 0.2039216f, 0.2588235f);
        dummy.transform.GetChild(0).gameObject.SetActive(false);
        dummy.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void InitDummy()
    {
        var container = transform.GetChild(3);
        for (int i = 0; i < carCount; i++)
        {
            carDummyList.Add(container.GetChild(i).gameObject);
        }

        for (int i = carCount; i < container.childCount; i++)
        {
            container.GetChild(i).gameObject.SetActive(false);
        }

        if (pointType != PointType.END) return;
        spawnDelayText.enabled = false;
        biasBackground.gameObject.SetActive(false);
        foreach (var carDummy in carDummyList)
        {
            HideDummy(carDummy);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        biasBackground = GetComponentInChildren<Image>();
        spawnDelayText = GetComponentsInChildren<Text>()[0];
        spawnBiasText = GetComponentsInChildren<Text>()[1];

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        ApplyTheme();
        ApplySprite();
        InitDummy();
        UpdateCarDummy();
        spawnDelayText.text = carSpawnDelay.ToString();

        audioSource.clip = sfxClip;

        if (pointType == PointType.START) StartCoroutine(StartSpawnCar());
    }
}
