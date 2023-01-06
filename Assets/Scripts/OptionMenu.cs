using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class OptionMenu : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Text resolutionText;
    public Slider volumeSlider;

    public AudioMixer audioMixer;

    private Resolution[] resolutionAry;
    private int resIndex;

    private void Init()
    {
        fullscreenToggle.isOn = Screen.fullScreen;
        vsyncToggle.isOn = QualitySettings.vSyncCount != 0;

        resolutionAry = Screen.resolutions;

        resIndex = resolutionAry.ToList().IndexOf(Screen.currentResolution);
        resolutionText.text = resolutionAry[resIndex].ToString();

        audioMixer.GetFloat("volume", out var currentVolume);
        volumeSlider.value = currentVolume;
    }

    private void Start()
    {
        Init();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void NextRes()
    {
        resIndex++;
        resIndex %= resolutionAry.Length;
        resolutionText.text = resolutionAry[resIndex].ToString();
    }

    public void PrevRes()
    {
        resIndex--;
        if (resIndex < 0) resIndex = resolutionAry.Length + resIndex;
        resolutionText.text = resolutionAry[resIndex].ToString();
    }

    public void Apply()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        Screen.SetResolution(resolutionAry[resIndex].width, resolutionAry[resIndex].height, fullscreenToggle.isOn);
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
    }

    public void Discard()
    {
        Init();
        gameObject.SetActive(false);
    }
}
