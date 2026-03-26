using UnityEngine;
using System;
using System.Collections.Generic;
using SQLite;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    private bool isSaved = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // 💾 SAVE GAME END (QUY TRÌNH CHUẨN)
    // =====================================================
    public void SaveGameEnd()
    {
        Debug.Log($"[Database] Bắt đầu quy trình lưu... Trạng thái hiện tại: isSaved = {isSaved}");

        if (isSaved) return;

        // 1. Kiểm tra các thành phần bắt buộc
        if (GameManager.instance == null || DatabaseManager.db == null)
        {
            Debug.LogError("❌ Không thể save: GameManager hoặc DatabaseManager NULL!");
            return;
        }

        try
        {
            // 🔥 Lấy độ khó thực tế từ PlayerPrefs (0: Easy, 1: Normal, 2: Hard)
            int actualDifficulty = PlayerPrefs.GetInt("difficulty", 0);
            string pName = PlayerPrefs.GetString("playerName", "Player");

            // BƯỚC A: TẠO VÀ LƯU SESSION
            GameSessionData currentSession = new GameSessionData
            {
                playerName = pName,
                difficultyID = actualDifficulty,
                score = GameManager.instance.score,

                // ✅ SỬA LỖI: Chuyển float thành string bằng ToString()
                // "F1" giúp lấy 1 chữ số thập phân (Ví dụ: "125.5s")
                time = Time.timeSinceLevelLoad.ToString("F1") + "s",

                distance = (GameManager.instance.player != null) ? GameManager.instance.player.position.x : 0,
                currentHP = GameManager.instance.currentHearts
            };

            // Chèn vào DB để lấy ID tự động tăng
            DatabaseManager.db.Insert(currentSession);
            int generatedSessionID = currentSession.sessionID;

            Debug.Log($"[Database] Đã tạo Session ID: {generatedSessionID} cho chế độ: {actualDifficulty}");

            // BƯỚC B: TẠO VÀ LƯU ACHIEVEMENT LIÊN KẾT
            AchievementData achievement = new AchievementData
            {
                sessionID = generatedSessionID,
                coin = currentSession.score,
                distance = currentSession.distance,
                hp = currentSession.currentHP,

                // ✅ Lưu thời điểm thực tế đạt được kỷ lục
                time = DateTime.Now.ToString("HH:mm:ss")
            };

            int result = DatabaseManager.db.Insert(achievement);

            if (result > 0)
            {
                Debug.Log($"✅ [Database] Lưu thành công! Mode: {actualDifficulty} | Player: {pName}");
                isSaved = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ [Database] Lỗi nghiêm trọng khi lưu: " + e.Message);
        }
    }

    // =====================================================
    // 🏆 TRUY VẤN TOP SCORE THEO CHẾ ĐỘ (DÙNG CHO UI)
    // =====================================================
    public List<AchievementDisplayModel> GetTopScoresByMode(int mode, int limit = 10)
    {
        if (DatabaseManager.db == null) return new List<AchievementDisplayModel>();

        // JOIN bảng để lấy Tên từ Session và Điểm từ Achievement
        string sql = @"
            SELECT s.playerName, a.coin, a.distance, a.time 
            FROM AchievementData a
            JOIN GameSessionData s ON a.sessionID = s.sessionID
            WHERE s.difficultyID = ?
            ORDER BY a.coin DESC 
            LIMIT ?";

        return DatabaseManager.db.Query<AchievementDisplayModel>(sql, mode, limit);
    }

    // =========================
    // 🔄 HỆ THỐNG QUẢN LÝ FLAG
    // =========================
    public void ResetSave()
    {
        isSaved = false;
        Debug.Log("[Database] Reset Save Flag cho lượt chơi mới.");
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ResetSave();
    }
}

// Lớp model để hứng dữ liệu hiển thị UI
public class AchievementDisplayModel
{
    public string playerName { get; set; }
    public int coin { get; set; }
    public float distance { get; set; }
    public string time { get; set; }
}