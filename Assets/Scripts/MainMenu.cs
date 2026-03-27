using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("--- UI Panels ---")]
    public GameObject mainMenuPanel;
    public GameObject playMenuPanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject guidePanel;
    public GameObject achievementPanel;
    public GameObject noSaveWarningPanel;

    [Header("--- Music Settings UI ---")]
    public AudioSource musicSource; // Kéo object có AudioSource nhạc menu vào đây
    public Image musicBtnImage;
    public Sprite iconOn;
    public Sprite iconOff;

    [Header("--- Player Name ---")]
    public TMP_InputField nameInput;

    [Header("--- Achievement System ---")]
    public AchievementUI achievementUI;
    public GameObject achievementButtonsPanel;

    [Header("--- Scene Names ---")]
    public string gameSceneName = "game";
    public string loadingSceneName = "LoadingSence";

    private bool isMusicOn = true;

    void Start()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);

        // Chỉ Load cài đặt Music
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;

        ApplyAudioSettings();
        ShowMainMenu();

        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    // --- LOGIC ÂM THANH (CHỈ GIỮ MUSIC) ---
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        // Điều khiển trực tiếp AudioSource nhạc nền
        if (musicSource != null)
        {
            musicSource.mute = !isMusicOn;
        }

        // Cập nhật Icon nút bấm Music
        if (musicBtnImage != null && iconOn != null && iconOff != null)
        {
            musicBtnImage.sprite = isMusicOn ? iconOn : iconOff;
        }
    }

    // --- LOGIC VÀO GAME ---
    public void StartEasy() { StartNewGame(0); }
    public void StartNormal() { StartNewGame(1); }
    public void StartHard() { StartNewGame(2); }

    private void StartNewGame(int difficulty)
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        PlayerPrefs.SetInt("difficulty", difficulty);

        string pName = (nameInput != null && !string.IsNullOrEmpty(nameInput.text)) ? nameInput.text : "Player";
        PlayerPrefs.SetString("playerName", pName);
        PlayerPrefs.Save();

        LoadWithLoadingScreen(gameSceneName);
    }

    public void ContinueGame()
    {
        string pName = (nameInput != null && !string.IsNullOrEmpty(nameInput.text)) ? nameInput.text : "Player";

        if (AchievementManager.instance != null && AchievementManager.instance.CheckHasSaveData(pName))
        {
            PlayerPrefs.SetString("playerName", pName);
            PlayerPrefs.SetInt("IsLoadingSave", 1);
            PlayerPrefs.Save();
            LoadWithLoadingScreen(gameSceneName);
        }
        else
        {
            if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(true);
        }
    }

    public void LoadWithLoadingScreen(string targetSceneName)
    {
        LoadingManager.SceneToLoad = targetSceneName;
        SceneManager.LoadScene(loadingSceneName);
    }

    // --- QUẢN LÝ PANEL UI ---
    private void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (playMenuPanel) playMenuPanel.SetActive(false);
        if (difficultyPanel) difficultyPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (guidePanel) guidePanel.SetActive(false);
        if (achievementPanel) achievementPanel.SetActive(false);
        if (noSaveWarningPanel) noSaveWarningPanel.SetActive(false);
    }

    public void ShowMainMenu() { HideAllPanels(); if (mainMenuPanel) mainMenuPanel.SetActive(true); }
    public void OpenPlayMenu() { HideAllPanels(); playMenuPanel.SetActive(true); }
    public void OpenDifficulty() { HideAllPanels(); difficultyPanel.SetActive(true); }
    public void OpenSettings() { HideAllPanels(); settingsPanel.SetActive(true); }
    public void OpenGuide() { HideAllPanels(); if (guidePanel) guidePanel.SetActive(true); }

    public void OpenAchievement()
    {
        HideAllPanels();
        if (achievementPanel)
        {
            achievementPanel.SetActive(true);
            if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
            if (achievementUI) achievementUI.ClearContent();
        }
    }

    public void OpenAchievementEasy() { ShowAchievementByMode(0); }
    public void OpenAchievementNormal() { ShowAchievementByMode(1); }
    public void OpenAchievementHard() { ShowAchievementByMode(2); }

    private void ShowAchievementByMode(int mode)
    {
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(false);
        if (achievementUI) achievementUI.ShowTopByMode(mode);
    }

    public void CloseNoSaveWarning() { if (noSaveWarningPanel) noSaveWarningPanel.SetActive(false); }
    public void BackToPlayMenu() { HideAllPanels(); playMenuPanel.SetActive(true); }
    public void BackToMainMenu() { ShowMainMenu(); }
    public void QuitGame() { Application.Quit(); }
}