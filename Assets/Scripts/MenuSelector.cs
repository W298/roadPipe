using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour
{
    public LoadingPanel loadingPanel;

    public void LoadStageSelectScene()
    {
        loadingPanel.LoadScene("StageSelect");
    }

    public void LoadOptionScene()
    {

    }

    public void ReloadCurrentScene()
    {
        loadingPanel.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadMainMenuScene()
    {
        loadingPanel.LoadScene("MainMenu");
    }
}
