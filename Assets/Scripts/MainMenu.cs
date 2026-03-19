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
    public string gameSceneName = "game"; // Đổi thành "game" cho khớp với ảnh Build của bạn
    public string loadingSceneName = "LoadingSence"; // Khớp chính xác tên bạn đặt

    [Header("Achievement UI")]
    public AchievementUI achievementUI;

    [Header("Achievement Buttons Panel")]
    public GameObject achievementButtonsPanel;

    void Start()
    {
        ShowMainMenu();
    }

    // --- HỆ THỐNG LOAD SCENE TRUNG GIAN ---
    // Hàm này sẽ được gọi thay vì SceneManager.LoadScene trực tiếp
    public void LoadWithLoadingScreen(string targetSceneName)
    {
        // 1. Ghi nhớ cảnh muốn đến vào biến tĩnh của LoadingManager
        // Lưu ý: Đảm bảo bạn đã tạo biến 'public static string SceneToLoad' trong script LoadingManager
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

        // ✅ Bật lại nút Achievement
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

    // ========================
    // NEW GAME (HỖ TRỢ LOADING SCREEN)
    // ========================

    public void StartEasy() { StartNewGame(0); }
    public void StartNormal() { StartNewGame(1); }
    public void StartHard() { StartNewGame(2); }

    void StartNewGame(int difficulty)
    {
        // Lưu thông số độ khó
        PlayerPrefs.SetInt("difficulty", difficulty);

        // Lưu tên người chơi
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
            PlayerPrefs.SetString("playerName", nameInput.text);
        else
            PlayerPrefs.SetString("playerName", "Player");

        PlayerPrefs.Save();

        Debug.Log("Start NEW Game Difficulty: " + difficulty);

        // Gọi màn hình Loading thay vì vào thẳng game
        LoadWithLoadingScreen(gameSceneName);
    }

    // ========================
    // CONTINUE & UTILS
    // ========================

    public void ContinueGame()
    {
        Debug.Log("Continue Game");
        LoadWithLoadingScreen(gameSceneName);
    }

    // Hàm này dùng cho nút "Back to Main Menu" từ trong Game Scene
    public void GoToMainMenu()
    {
        LoadWithLoadingScreen("Main Menu"); // Khớp với tên Scene Menu trong Build Settings
    }
}