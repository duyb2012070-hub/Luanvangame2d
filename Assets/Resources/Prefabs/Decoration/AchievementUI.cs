using UnityEngine;
using TMPro;
using System.Collections.Generic;
using SQLite;

public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform content;       // Scroll view content
    public GameObject itemPrefab;   // Prefab hiển thị 1 achievement

    [Header("Mode Buttons")]
    public UnityEngine.UI.Button easyButton;
    public UnityEngine.UI.Button normalButton;
    public UnityEngine.UI.Button hardButton;

    // Class phụ để nhận dữ liệu từ câu lệnh JOIN SQL (ĐÃ CẬP NHẬT)
    public class AchievementViewModel
    {
        public string playerName { get; set; }
        public int coin { get; set; }
        public float distance { get; set; }
        // public int hp { get; set; } // ĐÃ XÓA
        public float playTime { get; set; } // THÊM MỚI: Tổng thời gian đã chơi (giây)
        public string time { get; set; }     // Thời điểm thực hiện save
        public int difficultyID { get; set; }
    }

    void Start()
    {
        ClearContent();

        if (easyButton != null) easyButton.onClick.AddListener(() => ShowTopByMode(0));
        if (normalButton != null) normalButton.onClick.AddListener(() => ShowTopByMode(1));
        if (hardButton != null) hardButton.onClick.AddListener(() => ShowTopByMode(2));

        ShowTopByMode(0);
    }

    public void ClearContent()
    {
        if (content == null) return;
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    public void ShowTopByMode(int mode)
    {
        if (DatabaseManager.db == null)
        {
            Debug.LogError("Database chưa sẵn sàng!");
            return;
        }

        ClearContent();

        // CÂU LỆNH SQL JOIN (ĐÃ SỬA CỘT HP -> PLAYTIME)
        // Lưu ý: Giả sử playTime nằm trong bảng AchievementData hoặc GameSessionData của bạn
        string sql = @"
            SELECT s.playerName, a.coin, a.distance, a.playTime, a.time, s.difficultyID 
            FROM AchievementData a
            JOIN GameSessionData s ON a.sessionID = s.sessionID
            WHERE s.difficultyID = ?
            ORDER BY a.coin DESC 
            LIMIT 10";

        List<AchievementViewModel> list = DatabaseManager.db.Query<AchievementViewModel>(sql, mode);

        if (list == null || list.Count == 0)
        {
            ShowEmptyMessage();
            return;
        }

        int rank = 1;
        foreach (var data in list)
        {
            GameObject item = Instantiate(itemPrefab, content);
            TMP_Text txt = item.GetComponentInChildren<TMP_Text>();

            if (txt != null)
            {
                string modeName = GetModeName(mode);
                // Chuyển đổi giây sang định dạng MM:SS
                string formattedPlayTime = FormatSeconds(data.playTime);

                txt.text = $"<b>TOP {rank}</b>\n" +
                           $"NAME: {data.playerName}\n" +
                           $"COIN: {data.coin}\n" +
                           $"DIST: {data.distance:F1}m\n" +
                           $"PLAYED: {formattedPlayTime}\n" + // Hiển thị thời gian đã chơi
                           $"MODE: {modeName}\n" +
                           $"DATE: {data.time}"; // Ngày giờ thực tế khi lưu

                if (rank == 1) txt.color = Color.yellow;
                else 
               txt.color = new Color(0.8f, 0.5f, 0.2f);    // Đồng
            }
            rank++;
        }
    }

    // Hàm bổ trợ chuyển đổi giây -> Phút:Giây
    private string FormatSeconds(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60);
        int secs = Mathf.FloorToInt(totalSeconds % 60);
        return string.Format("{0:00}:{1:00}", mins, secs);
    }

    private string GetModeName(int mode)
    {
        return mode switch
        {
            0 => "Easy",
            1 => "Normal",
            2 => "Hard",
            _ => "Unknown"
        };
    }

    private void ShowEmptyMessage()
    {
        GameObject item = Instantiate(itemPrefab, content);
        TMP_Text txt = item.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = "Chưa có dữ liệu cho chế độ này!";
    }
}