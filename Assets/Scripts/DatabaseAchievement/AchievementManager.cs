using UnityEngine;
using System;
using System.Collections.Generic;
using SQLite;
using UnityEngine.SceneManagement;

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
    // 💾 HỆ THỐNG SAVE/LOAD TRẠNG THÁI (Lưu ngầm giữa trận)
    // =====================================================
    public void SaveCurrentGame(SaveGameData data)
    {
        if (DatabaseManager.db == null) return;

        try
        {
            // Xóa bản lưu cũ của người chơi này và chèn bản mới
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", data.playerName);
            DatabaseManager.db.Insert(data);
            Debug.Log($"💾 [Database] Đã lưu TRẠNG THÁI chơi cho {data.playerName}. Thời gian: {data.playTime}s");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Lỗi khi SaveCurrentGame: " + e.Message);
        }
    }

    public SaveGameData GetPlayerData(string pName)
    {
        if (DatabaseManager.db == null) return null;
        try
        {
            return DatabaseManager.db.Table<SaveGameData>()
                .Where(s => s.playerName == pName)
                .FirstOrDefault();
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Lỗi khi GetPlayerData: " + e.Message);
            return null;
        }
    }

    // =====================================================
    // 🏆 SAVE GAME END (Lưu kỷ lục khi GAME OVER)
    // =====================================================
    public void SaveGameEnd()
    {
        if (isSaved) return;

        if (GameManager.instance == null || DatabaseManager.db == null)
        {
            Debug.LogError("❌ Không thể save: GameManager hoặc DatabaseManager NULL!");
            return;
        }

        try
        {
            int actualDifficulty = PlayerPrefs.GetInt("difficulty", 0);
            string pName = PlayerPrefs.GetString("playerName", "Player");

            // Lấy thời gian từ playTimer của GameManager mà bạn đã sửa
            float finalPlaySeconds = GameManager.instance.playTimer;

            // BƯỚC A: LƯU SESSION (Thông tin tổng quát ván chơi)
            GameSessionData currentSession = new GameSessionData
            {
                playerName = pName,
                difficultyID = actualDifficulty,
                score = GameManager.instance.score,
                // Lưu chuỗi để debug trong DB (ví dụ: "120.5s")
                time = finalPlaySeconds.ToString("F1") + "s",
                distance = (GameManager.instance.player != null) ? GameManager.instance.player.position.x : 0,
                currentHP = 0
            };

            DatabaseManager.db.Insert(currentSession);
            int generatedSessionID = currentSession.sessionID;

            // BƯỚC B: LƯU ACHIEVEMENT (Dữ liệu cho Bảng xếp hạng)
            AchievementData achievement = new AchievementData
            {
                sessionID = generatedSessionID,
                coin = GameManager.instance.score,
                distance = currentSession.distance,
                hp = 0, // Kết thúc game mặc định hp = 0

                // 1. Lưu số giây thực tế để UI tính toán 00:00
                playTime = finalPlaySeconds,

                // 2. Lưu ngày giờ hệ thống để biết chơi khi nào
                time = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            int result = DatabaseManager.db.Insert(achievement);

            if (result > 0)
            {
                Debug.Log($"✅ [Database] Đã lưu KỶ LỤC: {finalPlaySeconds}s vào lúc {achievement.time}");
                isSaved = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ [Database] Lỗi khi lưu GameEnd: " + e.Message);
        }
    }

    // =====================================================
    // 📊 TRUY VẤN UI (DÙNG CHO BẢNG THÀNH TỰU)
    // =====================================================
    public List<AchievementDisplayModel> GetTopScoresByMode(int mode, int limit = 10)
    {
        if (DatabaseManager.db == null) return new List<AchievementDisplayModel>();

        // Lấy thêm trường playTime (số giây) và time (ngày giờ)
        string sql = @"
            SELECT s.playerName, a.coin, a.distance, a.playTime, a.time 
            FROM AchievementData a
            JOIN GameSessionData s ON a.sessionID = s.sessionID
            WHERE s.difficultyID = ?
            ORDER BY a.coin DESC 
            LIMIT ?";

        return DatabaseManager.db.Query<AchievementDisplayModel>(sql, mode, limit);
    }

    // =====================================================
    // 🔄 QUẢN LÝ FLAG & SCENE LOAD
    // =====================================================
    public void ResetSave() { isSaved = false; }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ResetSave(); }

    public bool CheckHasSaveData(string pName)
    {
        if (DatabaseManager.db == null) return false;
        try
        {
            var data = DatabaseManager.db.Table<SaveGameData>()
                .Where(s => s.playerName == pName)
                .FirstOrDefault();
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Lỗi khi CheckHasSaveData: " + e.Message);
            return false;
        }
    }
}

// Model hiển thị trên UI: Phải có đủ cả 2 loại thời gian
public class AchievementDisplayModel
{
    public string playerName { get; set; }
    public int coin { get; set; }
    public float distance { get; set; }

    // Dùng để script UI format thành "02:30"
    public float playTime { get; set; }

    // Dùng để hiện ngày tháng "06/04/2026"
    public string time { get; set; }
}