using StageSelectorUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{
    private Button backButton;
    private Button retryButton;

    private void BackToStageSelect()
    {
        GameManager.instance.SaveScore();
        SceneManager.LoadScene("StageSelect");
    }

    private void Retry()
    {
        GameManager.instance.SaveScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        backButton = transform.GetChild(0).GetComponent<Button>();
        retryButton = transform.GetChild(1).GetComponent<Button>();

        backButton.onClick.AddListener(BackToStageSelect);
        retryButton.onClick.AddListener(Retry);
    }
}
