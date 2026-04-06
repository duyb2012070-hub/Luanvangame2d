using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public GameObject achievementButtonsPanel; // Chứa 3 nút chọn Mode
    public AchievementUI achievementUI;        // Script hiển thị danh sách điểm

    [Header("--- Music Settings UI ---")]
    public AudioSource musicSource;
    public Image musicBtnImage;
    public Sprite musicIconOn;
    public Sprite musicIconOff;

    [Header("--- Sound Effect Settings UI ---")]
    public Image sfxBtnImage;
    public Sprite sfxIconOn;
    public Sprite sfxIconOff;
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
        // QUAN TRỌNG: Đảm bảo thời gian chạy bình thường (Fix lỗi GameTimer từ scene trước)
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("IsLoadingSave", 0);

        // Load cấu hình âm thanh
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSfxOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;

        // Tự động điền tên cũ nếu có (Tiện lợi cho người dùng)
        if (nameInput != null)
            nameInput.text = PlayerPrefs.GetString("playerName", "");

        ApplyAudioSettings();
        ApplySfxSettings();

        // Luôn bắt đầu tại Menu chính
        ShowMainMenu();

        // Đảm bảo các bảng thông báo luôn ẩn lúc khởi đầu
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
        if (storyPanel != null) storyPanel.SetActive(false);
    }

    #region HỆ THỐNG ÂM THANH
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
        if (musicBtnImage != null)
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
        if (sfxBtnImage != null)
            sfxBtnImage.sprite = isSfxOn ? sfxIconOn : sfxIconOff;

        foreach (AudioSource source in uiAudioSources)
        {
            if (source != null) source.mute = !isSfxOn;
        }
    }
    #endregion

    #region QUẢN LÝ PANEL UI (ẨN/HIỆN)
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

        // Reset trạng thái các nút Achievement khi quay về Menu chính
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
    }

    public void OpenPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }
    public void OpenDifficulty() { HideAllPanels(); if (difficultyPanel) difficultyPanel.SetActive(true); }
    public void BackToPlayMenu() { HideAllPanels(); if (playMenuPanel) playMenuPanel.SetActive(true); }
    public void OpenSettings() { HideAllPanels(); if (settingsPanel) settingsPanel.SetActive(true); }
    public void OpenGuide() { HideAllPanels(); if (guidePanel) guidePanel.SetActive(true); }
    public void OpenStory() { HideAllPanels(); if (storyPanel) storyPanel.SetActive(true); }
    public void BackFromStory() => ShowMainMenu();
    public void CloseNoSaveWarning() { if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false); }
    #endregion

    #region LOGIC CHƠI GAME
    public void ContinueGame()
    {
        string pName = (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
                       ? nameInput.text
                       : PlayerPrefs.GetString("playerName", "");

        if (string.IsNullOrEmpty(pName))
        {
            if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(true);
            return;
        }

        if (CheckNameExists(pName))
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
    #endregion

    #region HỆ THỐNG ACHIEVEMENT (BẢNG XẾP HẠNG)
    public void OpenAchievement()
    {
        HideAllPanels();
        if (achievementPanel)
        {
            achievementPanel.SetActive(true);
            // Đảm bảo hiện nút chọn Mode khi mở bảng
            if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
            if (achievementUI) achievementUI.ClearContent();
        }
    }

    public void OpenAchievementEasy() => ShowAchievementByMode(0);
    public void OpenAchievementNormal() => ShowAchievementByMode(1);
    public void OpenAchievementHard() => ShowAchievementByMode(2);

    private void ShowAchievementByMode(int mode)
    {
        // Ẩn nút Mode để hiện danh sách điểm
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(false);
        if (achievementUI) achievementUI.ShowTopByMode(mode);
    }

    public void BackFromTopScores()
    {
        // Hiện lại nút Mode và dọn dẹp danh sách
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
        if (achievementUI) achievementUI.ClearContent();
    }

    public void BackFromAchievement()
    {
        ShowMainMenu();
    }
    #endregion

    public void QuitGame() => Application.Quit();
}