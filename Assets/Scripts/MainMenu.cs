using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject playMenuPanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject guidePanel;
    public GameObject achievementPanel;

    [Header("Player Name")]
    public TMP_InputField nameInput;

    [Header("Scene")]
    public string gameSceneName = "Game";

    public AchievementUI achievementUI;
    void Start()
    {
        ShowMainMenu();
    }

    void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (playMenuPanel) playMenuPanel.SetActive(false);
        if (difficultyPanel) difficultyPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (guidePanel) guidePanel.SetActive(false);
        if (achievementPanel) achievementPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void OpenPlayMenu()
    {
        HideAllPanels();
        playMenuPanel.SetActive(true);
    }

    public void OpenDifficulty()
    {
        HideAllPanels();
        difficultyPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    public void OpenGuide()
    {
        HideAllPanels();
        guidePanel.SetActive(true);
    }

    

    public void BackToPlayMenu()
    {
        HideAllPanels();
        playMenuPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    public void ResetName()
    {
        if (nameInput != null)
            nameInput.text = "";
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ChangeSFXVolume(float value)
    {
        AudioListener.volume = value;
    }
    public void OpenAchievement()
    {
        HideAllPanels();
        achievementPanel.SetActive(true);

        achievementUI.ShowLast10(); // 👉 gọi hiển thị
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //========================
    // NEW GAME (🔥 FIX CHUẨN)
    //========================

    public void StartEasy()
    {
        StartNewGame(0);
    }

    public void StartNormal()
    {
        StartNewGame(1);
    }

    public void StartHard()
    {
        StartNewGame(2);
    }

    void StartNewGame(int difficulty)
    {
        // 👉 LƯU DIFFICULTY (QUAN TRỌNG)
        PlayerPrefs.SetInt("difficulty", difficulty);

        // 👉 LƯU TÊN PLAYER
        if (nameInput != null && nameInput.text != "")
        {
            PlayerPrefs.SetString("playerName", nameInput.text);
        }
        else
        {
            PlayerPrefs.SetString("playerName", "Player");
        }

        PlayerPrefs.Save();

        Debug.Log("Start NEW Game Difficulty: " + difficulty);

        SceneManager.LoadScene(gameSceneName);
    }

    //========================
    // CONTINUE GAME
    //========================

    public void ContinueGame()
    {
        Debug.Log("Continue Game");

        SceneManager.LoadScene(gameSceneName);
    }
}