using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour
{
    public void LoadStageSelectScene()
    {
        SceneManager.LoadScene("StageSelect");
    }

    public void LoadOptionScene()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
