using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic; // Cần thiết để dùng List

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
    public GameObject storyPanel;

    [Header("--- Achievement Sub-Panels ---")]
    public GameObject achievementButtonsPanel;
    public AchievementUI achievementUI;

    [Header("--- Music Settings UI ---")]
    public AudioSource musicSource;
    public Image musicBtnImage;
    public Sprite musicIconOn;
    public Sprite musicIconOff;

    [Header("--- Sound Effect Settings UI ---")]
    public Image sfxBtnImage;
    public Sprite sfxIconOn;
    public Sprite sfxIconOff;
    // Thêm danh sách các AudioSource phát tiếng Button (Hover/Click)
    public List<AudioSource> uiAudioSources = new List<AudioSource>();

    [Header("--- Player Name Input ---")]
    public TMP_InputField nameInput;

    [Header("--- Scene Names ---")]
    public string gameSceneName = "game";
    public string loadingSceneName = "LoadingSence";

    private bool isMusicOn = true;
    private bool isSfxOn = true;

    void Start()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);

        // Load cấu hình âm thanh
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSfxOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;

        ApplyAudioSettings();
        ApplySfxSettings();

        ShowMainMenu();

        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
        if (storyPanel != null) storyPanel.SetActive(false);
    }

    // ==========================================
    // --- HỆ THỐNG ÂM THANH (MUSIC & SFX) ---
    // ==========================================

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
        if (musicBtnImage != null && musicIconOn != null && musicIconOff != null)
            musicBtnImage.sprite = isMusicOn ? musicIconOn : musicIconOff;
    }

    public void ToggleSfx()
    {
        isSfxOn = !isSfxOn;
        PlayerPrefs.SetInt("SfxOn", isSfxOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplySfxSettings();
    }

    private void ApplySfxSettings()
    {
        // 1. Cập nhật Icon nút bấm
        if (sfxBtnImage != null && sfxIconOn != null && sfxIconOff != null)
            sfxBtnImage.sprite = isSfxOn ? sfxIconOn : sfxIconOff;

        // 2. Mute/Unmute toàn bộ AudioSource trong danh sách UI
        foreach (AudioSource source in uiAudioSources)
        {
            if (source != null)
            {
                source.mute = !isSfxOn;
            }
        }
    }

    // ==========================================
    // --- QUẢN LÝ PANEL & STORY ---
    // ==========================================

    public void OpenStory()
    {
        HideAllPanels();
        if (storyPanel != null) storyPanel.SetActive(true);
    }

    public void BackFromStory()
    {
        ShowMainMenu();
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
        if (storyPanel) storyPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
    }

    // ==========================================
    // --- LOGIC CHƠI GAME (CONTINUE & NEW) ---
    // ==========================================

    public void ContinueGame()
    {
        string pName = PlayerPrefs.GetString("playerName", "");

        if (string.IsNullOrEmpty(pName))
        {
            if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(true);
            return;
        }

        if (CheckNameExists(pName))
        {
            PlayerPrefs.SetInt("IsLoadingSave", 1);
            PlayerPrefs.Save();
            LoadWithLoadingScreen(gameSceneName);
        }
        else
        {
            if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(true);
        }
    }

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

    // ==========================================
    // --- CÁC PANEL PHỤ ---
    // ==========================================

    public void OpenPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }
    public void OpenDifficulty() { HideAllPanels(); if (difficultyPanel) difficultyPanel.SetActive(true); }
    public void OpenSettings() { HideAllPanels(); if (settingsPanel) settingsPanel.SetActive(true); }
    public void OpenGuide() { HideAllPanels(); if (guidePanel) guidePanel.SetActive(true); }
    public void BackToPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }
    public void CloseNoSaveWarning() { if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false); }

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

    public void QuitGame() { Application.Quit(); }
}