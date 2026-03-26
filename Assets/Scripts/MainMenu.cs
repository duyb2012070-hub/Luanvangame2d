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

    [Header("Scene Names")]
    public string gameSceneName = "game";
    public string loadingSceneName = "LoadingSence";

    [Header("Achievement UI")]
    public AchievementUI achievementUI;

    [Header("Achievement Buttons Panel")]
    public GameObject achievementButtonsPanel;

    void Start()
    {
        ShowMainMenu();

        // Mẹo: Khi bắt đầu Menu, bạn có thể xóa các PlayerPrefs cũ 
        // để đảm bảo không còn dữ liệu "rác" từ hệ thống Continue cũ.
        // PlayerPrefs.DeleteKey("lastPosition"); 
    }

    // --- HỆ THỐNG LOAD SCENE TRUNG GIAN ---
    public void LoadWithLoadingScreen(string targetSceneName)
    {
        // 1. Ghi nhớ cảnh muốn đến
        LoadingManager.SceneToLoad = targetSceneName;

        // 2. Mở Scene Loading
        SceneManager.LoadScene(loadingSceneName);
    }

    // --- QUẢN LÝ PANEL ---
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

        if (achievementButtonsPanel != null)
            achievementButtonsPanel.SetActive(true);
    }

    public void OpenPlayMenu() { HideAllPanels(); playMenuPanel.SetActive(true); }
    public void OpenDifficulty() { HideAllPanels(); difficultyPanel.SetActive(true); }
    public void OpenSettings() { HideAllPanels(); settingsPanel.SetActive(true); }
    public void OpenGuide() { HideAllPanels(); guidePanel.SetActive(true); }
    public void BackToPlayMenu() { HideAllPanels(); playMenuPanel.SetActive(true); }
    public void BackToMainMenu() { ShowMainMenu(); }

    // --- SETTINGS ---
    public void ResetName() { if (nameInput != null) nameInput.text = ""; }
    public void ToggleFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }
    public void ChangeSFXVolume(float value) { AudioListener.volume = value; }

    // --- ACHIEVEMENT ---
    public void OpenAchievement()
    {
        HideAllPanels();
        achievementPanel.SetActive(true);
        if (achievementButtonsPanel != null) achievementButtonsPanel.SetActive(true);
        if (achievementUI != null) achievementUI.ClearContent();
    }

    public void OpenAchievementEasy() { ShowAchievementByMode(0); }
    public void OpenAchievementNormal() { ShowAchievementByMode(1); }
    public void OpenAchievementHard() { ShowAchievementByMode(2); }

    private void ShowAchievementByMode(int mode)
    {
        HideAllPanels();
        achievementPanel.SetActive(true);
        if (achievementButtonsPanel != null) achievementButtonsPanel.SetActive(false);
        if (achievementUI != null) achievementUI.ShowTopByMode(mode);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =====================================================
    // 🎮 NEW GAME (CHỈ CÒN CHẾ ĐỘ CHƠI MỚI)
    // =====================================================

    public void StartEasy() { StartNewGame(0); }
    public void StartNormal() { StartNewGame(1); }
    public void StartHard() { StartNewGame(2); }

    void StartNewGame(int difficulty)
    {
        // 1. Lưu thông số độ khó mới
        PlayerPrefs.SetInt("difficulty", difficulty);

        // 2. Lưu tên người chơi mới
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
            PlayerPrefs.SetString("playerName", nameInput.text);
        else
            PlayerPrefs.SetString("playerName", "Player");

        // 3. Xóa các dữ liệu checkpoint cũ nếu có để tránh New Game mà bị nhảy checkpoint
        // PlayerPrefs.DeleteKey("checkpointX"); 

        PlayerPrefs.Save();

        Debug.Log($"[MainMenu] Bắt đầu chơi mới - Độ khó: {difficulty}");

        // 4. Vào game qua màn hình Loading
        LoadWithLoadingScreen(gameSceneName);
    }

    // --- TIỆN ÍCH ---
    public void GoToMainMenu()
    {
        LoadWithLoadingScreen("Main Menu");
    }
}