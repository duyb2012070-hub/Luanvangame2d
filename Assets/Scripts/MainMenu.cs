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
    public GameObject storyPanel; // <--- Thêm Panel Story ở đây

    [Header("--- Achievement Sub-Panels ---")]
    public GameObject achievementButtonsPanel;
    public AchievementUI achievementUI;

    [Header("--- Music Settings UI ---")]
    public AudioSource musicSource;
    public Image musicBtnImage;
    public Sprite iconOn;
    public Sprite iconOff;

    [Header("--- Player Name ---")]
    public TMP_InputField nameInput;

    [Header("--- Scene Names ---")]
    public string gameSceneName = "game";
    public string loadingSceneName = "LoadingSence";

    private bool isMusicOn = true;

    void Start()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;

        ApplyAudioSettings();
        ShowMainMenu();

        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
        // Ẩn story panel khi bắt đầu
        if (storyPanel != null) storyPanel.SetActive(false);
    }

    // --- LOGIC MỞ VÀ ĐÓNG STORY PANEL ---
    public void OpenStory()
    {
        HideAllPanels();
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }
    }

    public void BackFromStory()
    {
        ShowMainMenu(); // Quay lại menu chính
    }

    // --- LOGIC TIẾP TỤC GAME (CONTINUE) ---
    public void ContinueGame()
    {
        // 1. Lấy tên từ PlayerPrefs (đã được NameInputHandler cập nhật khi gõ)
        string pName = PlayerPrefs.GetString("playerName", "");

        // 2. Nếu tên trống thì hiện cảnh báo ngay
        if (string.IsNullOrEmpty(pName))
        {
            if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(true);
            return;
        }

        // 3. KIỂM TRA TRONG DATABASE (Bước quan trọng bị thiếu)
        if (CheckNameExists(pName))
        {
            // Nếu có dữ liệu -> Cho phép vào game
            PlayerPrefs.SetInt("IsLoadingSave", 1);
            PlayerPrefs.Save();
            LoadWithLoadingScreen(gameSceneName);
        }
        else
        {
            // Nếu KHÔNG có dữ liệu -> Hiện Panel thông báo lỗi
            Debug.LogWarning("Không tìm thấy dữ liệu cho tên: " + pName);
            if (noSaveWarningPanel != null)
            {
                noSaveWarningPanel.SetActive(true);
            }
        }
    }

    // Hàm phụ để kiểm tra DB (Giống bên NameInputHandler)
    private bool CheckNameExists(string nameToCheck)
    {
        if (DatabaseManager.db == null) return false;
        try
        {
            var result = DatabaseManager.db.Table<SaveGameData>()
                .Where(v => v.playerName.ToLower() == nameToCheck.ToLower()).FirstOrDefault();
            return result != null;
        }
        catch { return false; }
    }

    public void CloseNoSaveWarning()
    {
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (playMenuPanel) playMenuPanel.SetActive(false);
        if (difficultyPanel) difficultyPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (guidePanel) guidePanel.SetActive(false);
        if (achievementPanel) achievementPanel.SetActive(false);
        if (noSaveWarningPanel) noSaveWarningPanel.SetActive(false);
        if (storyPanel) storyPanel.SetActive(false); // Ẩn luôn story panel
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
    }

    // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
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

    public void BackFromTopScores()
    {
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
        if (achievementUI) achievementUI.ClearContent();
    }

    public void OpenPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }
    public void OpenDifficulty() { HideAllPanels(); if (difficultyPanel) difficultyPanel.SetActive(true); }
    public void OpenSettings() { HideAllPanels(); if (settingsPanel) settingsPanel.SetActive(true); }
    public void OpenGuide() { HideAllPanels(); if (guidePanel) guidePanel.SetActive(true); }
    public void BackToPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }

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

    public void LoadWithLoadingScreen(string targetSceneName)
    {
        LoadingManager.SceneToLoad = targetSceneName;
        SceneManager.LoadScene(loadingSceneName);
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        if (musicSource != null) musicSource.mute = !isMusicOn;
        if (musicBtnImage != null && iconOn != null && iconOff != null)
            musicBtnImage.sprite = isMusicOn ? iconOn : iconOff;
    }

    public void QuitGame() { Application.Quit(); }
}