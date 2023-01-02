using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreIndicator : MonoBehaviour
{
    private static ScoreIndicator _instance;
    public static ScoreIndicator instance => _instance ??= FindObjectOfType<ScoreIndicator>();

    public Sprite star;
    public Sprite fillStar;
    public Sprite halfStar;

    private Image[] imageAry;

    public void Render(int score)
    {
        for (int i = 0; i < score; i++)
        {
            imageAry[i].sprite = fillStar;
        }

        for (int i = score; i < imageAry.Length; i++)
        {
            imageAry[i].sprite = star;
        }
    }

    private void Awake()
    {
        imageAry = GetComponentsInChildren<Image>();
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
