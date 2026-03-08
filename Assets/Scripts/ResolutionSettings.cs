using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    public GameObject resolutionPanel;

    // MỞ PANEL RESOLUTION
    public void OpenResolution()
    {
        resolutionPanel.SetActive(true);
    }

    // ĐÓNG PANEL RESOLUTION
    public void CloseResolution()
    {
        resolutionPanel.SetActive(false);
    }

    // RESOLUTION 1920x1080
    public void Set1920x1080()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }

    // RESOLUTION 1600x900
    public void Set1600x900()
    {
        Screen.SetResolution(1600, 900, FullScreenMode.Windowed);
    }

    // RESOLUTION 1280x720
    public void Set1280x720()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }
}