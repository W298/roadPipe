using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingPanel : MonoBehaviour
{
    public GameObject bgPanel;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        
        bgPanel.SetActive(true);
        while (!op.isDone)
        {
            yield return null;
        }
    }
}
