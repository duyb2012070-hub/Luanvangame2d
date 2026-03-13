using UnityEngine;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject resolutionPanel;

    bool soundOn = true;
    bool musicOn = true;

    public TMP_Text soundText;
    public TMP_Text musicText;

    // MỞ SETTINGS
    public void OpenSettings()
    {
        settingPanel.SetActive(true);
    }

    // ĐÓNG SETTINGS
    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }

    // BẬT TẮT SOUND
    public void ToggleSound()
    {
        soundOn = !soundOn;
        soundText.text = "Sound: " + (soundOn ? "ON" : "OFF");
    }

    // BẬT TẮT MUSIC
    public void ToggleMusic()
    {
        musicOn = !musicOn;
        musicText.text = "Music: " + (musicOn ? "ON" : "OFF");
    }

    // MỞ MENU RESOLUTION
    public void OpenResolution()
    {
        resolutionPanel.SetActive(true);
    }

    // ĐÓNG MENU RESOLUTION
    public void CloseResolution()
    {
        resolutionPanel.SetActive(false);
    }

    // ĐỔI ĐỘ PHÂN GIẢI
    public void SetResolution1920()
    {
        Screen.SetResolution(1920, 1080, true);
    }

    public void SetResolution1600()
    {
        Screen.SetResolution(1600, 900, true);
    }

    public void SetResolution1280()
    {
        Screen.SetResolution(1280, 720, true);
    }
}