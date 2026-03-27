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
    public GameObject noSaveWarningPanel; // <--- Panel thông báo sẽ hiện ở đây

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

        // Luôn ẩn panel cảnh báo khi mới vào game
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    // --- LOGIC TIẾP TỤC GAME (CONTINUE) ---
    public void ContinueGame()
    {
        // Lấy tên người chơi, nếu trống thì mặc định là "Player"
        string pName = (nameInput != null && !string.IsNullOrEmpty(nameInput.text)) ? nameInput.text : "Player";

        // Kiểm tra trong AchievementManager (hoặc Database) xem có dữ liệu của pName này không
        if (AchievementManager.instance != null && AchievementManager.instance.CheckHasSaveData(pName))
        {
            Debug.Log($"<color=green>📂 Tìm thấy dữ liệu cho: {pName}. Đang tải game...</color>");
            PlayerPrefs.SetString("playerName", pName);
            PlayerPrefs.SetInt("IsLoadingSave", 1);
            PlayerPrefs.Save();
            LoadWithLoadingScreen(gameSceneName);
        }
        else
        {
            // NẾU KHÔNG CÓ DỮ LIỆU -> Hiện bảng thông báo
            Debug.LogWarning($"⚠️ Không tìm thấy dữ liệu lưu cho: {pName}");
            if (noSaveWarningPanel != null)
            {
                noSaveWarningPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("❌ Bạn chưa kéo thả 'NoSaveWarningPanel' vào script MainMenu trong Inspector!");
            }
        }
    }

    // Hàm để gán vào nút "Đóng" hoặc "OK" trên Panel cảnh báo
    public void CloseNoSaveWarning()
    {
        if (noSaveWarningPanel != null) noSaveWarningPanel.SetActive(false);
    }

    // --- CÁC HÀM QUẢN LÝ PANEL KHÁC (GIỮ NGUYÊN) ---

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

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (achievementButtonsPanel) achievementButtonsPanel.SetActive(true);
    }

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