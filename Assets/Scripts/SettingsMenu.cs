using UnityEngine;
using TMPro;
using SQLite;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingPanel;
    public GameObject resolutionPanel;

    [Header("Status UI")]
    public TMP_Text soundText;
    public TMP_Text musicText;

    // Biến nội bộ để theo dõi trạng thái hiện tại
    private bool soundOn = true;
    private bool musicOn = true;
    private string currentResolution = "1920x1080";

    void Start()
    {
        LoadSettingsFromDB();
    }

    // =====================================================
    // 💾 HỆ THỐNG LƯU & TẢI (DATABASE)
    // =====================================================

    public void LoadSettingsFromDB()
    {
        if (DatabaseManager.db == null) return;

        string pName = PlayerPrefs.GetString("playerName", "Player");

        // Tìm cài đặt của người chơi này trong DB
        var data = DatabaseManager.db.Table<SettingsData>()
                                     .FirstOrDefault(x => x.playerName == pName);

        if (data != null)
        {
            soundOn = data.soundOn == 1;
            musicOn = data.musicOn == 1;
            currentResolution = data.resolution;

            // Cập nhật UI ngay lập tức
            UpdateUI();
            Debug.Log($"[Settings] Đã tải cài đặt cho: {pName}");
        }
        else
        {
            Debug.Log("[Settings] Không tìm thấy dữ liệu cũ, dùng mặc định.");
        }
    }

    public void SaveSettingsToDB()
    {
        if (DatabaseManager.db == null) return;

        string pName = PlayerPrefs.GetString("playerName", "Player");

        SettingsData data = new SettingsData
        {
            playerName = pName,
            soundOn = soundOn ? 1 : 0,
            musicOn = musicOn ? 1 : 0,
            resolution = currentResolution
        };

        // InsertOrReplace: Nếu đã có playerName này thì Ghi đè, chưa có thì Thêm mới
        DatabaseManager.db.InsertOrReplace(data);
        Debug.Log($"[Settings] Đã lưu cài đặt vào DB cho: {pName}");
    }

    // =====================================================
    // 🕹️ ĐIỀU KHIỂN UI & LOGIC
    // =====================================================

    private void UpdateUI()
    {
        soundText.text = "Sound: " + (soundOn ? "ON" : "OFF");
        musicText.text = "Music: " + (musicOn ? "ON" : "OFF");
    }

    public void OpenSettings()
    {
        settingPanel.SetActive(true);
        LoadSettingsFromDB(); // Tải lại mỗi khi mở để đảm bảo đồng bộ
    }

    public void CloseSettings()
    {
        SaveSettingsToDB(); // Tự động lưu khi đóng bảng cài đặt
        settingPanel.SetActive(false);
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        UpdateUI();
        // Bạn có thể thêm code để tắt AudioListener ở đây nếu muốn hiệu ứng tức thì
        AudioListener.pause = !soundOn;
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        UpdateUI();
        // Gửi sự kiện hoặc tìm MusicPlayer để tắt nhạc
    }

    // =====================================================
    // 🖥️ ĐỘ PHÂN GIẢI (RESOLUTION)
    // =====================================================

    public void OpenResolution() => resolutionPanel.SetActive(true);
    public void CloseResolution() => resolutionPanel.SetActive(false);

    public void SetResolution1920() { ApplyResolution(1920, 1080); }
    public void SetResolution1600() { ApplyResolution(1600, 900); }
    public void SetResolution1280() { ApplyResolution(1280, 720); }

    private void ApplyResolution(int width, int height)
    {
        Screen.SetResolution(width, height, true);
        currentResolution = $"{width}x{height}";
        Debug.Log($"Đã đổi độ phân giải: {currentResolution}");
    }
}