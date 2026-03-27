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
    // 💾 HỆ THỐNG SAVE/LOAD TRẠNG THÁI (CHO NÚT SAVE & CONTINUE)
    // =====================================================

    public void SaveCurrentGame(SaveGameData data)
    {
        if (DatabaseManager.db == null) return;

        try
        {
            // TẠI ĐÂY: Dữ liệu 'data' (SaveGameData) VẪN CÓ trường 'health' 
            // để người chơi Load game lại còn có máu để chơi tiếp.
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", data.playerName);
            DatabaseManager.db.Insert(data);
            Debug.Log($"💾 [Database] Đã lưu TRẠNG THÁI chơi cho {data.playerName}. Máu hiện tại: {data.health}");
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
    // 🏆 SAVE GAME END (LƯU KỶ LỤC KHI GAME OVER)
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

            // BƯỚC A: TẠO VÀ LƯU SESSION (Lưu lại thông số tổng quát của ván chơi)
            GameSessionData currentSession = new GameSessionData
            {
                playerName = pName,
                difficultyID = actualDifficulty,
                score = GameManager.instance.score,
                time = Time.timeSinceLevelLoad.ToString("F1") + "s",
                distance = (GameManager.instance.player != null) ? GameManager.instance.player.position.x : 0,
                // Chỗ này bạn có thể giữ currentHP trong Session để log, 
                // nhưng thường thì kết thúc game hp = 0 nên có thể bỏ qua.
                currentHP = 0
            };

            DatabaseManager.db.Insert(currentSession);
            int generatedSessionID = currentSession.sessionID;

            // BƯỚC B: TẠO VÀ LƯU ACHIEVEMENT (Bảng Thành Tựu/Kỷ Lục)
            // ĐÃ XÓA TRƯỜNG 'hp' HOẶC 'health' Ở ĐÂY
            AchievementData achievement = new AchievementData
            {
                sessionID = generatedSessionID,
                coin = currentSession.score,
                distance = currentSession.distance,
                // hp = ... (DÒNG NÀY ĐÃ BỊ XÓA VÌ KẾT THÚC GAME HP LUÔN = 0)
                time = DateTime.Now.ToString("HH:mm:ss")
            };

            int result = DatabaseManager.db.Insert(achievement);

            if (result > 0)
            {
                Debug.Log($"✅ [Database] Đã lưu KỶ LỤC thành công (Không lưu máu vì đã chết).");
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

        // Query này chỉ lấy Tên, Xu, Quãng đường, Thời gian để hiện lên bảng xếp hạng
        string sql = @"
            SELECT s.playerName, a.coin, a.distance, a.time 
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

    public void ResetSave()
    {
        isSaved = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetSave();
    }
    public bool CheckHasSaveData(string pName)
    {
        if (DatabaseManager.db == null) return false;
        try
        {
            // Tìm xem có bản ghi nào trong bảng SaveGameData không
            var data = DatabaseManager.db.Table<SaveGameData>()
                .Where(s => s.playerName == pName)
                .FirstOrDefault();

            return data != null; // Trả về true nếu có dữ liệu, false nếu không
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Lỗi khi CheckHasSaveData: " + e.Message);
            return false;
        }
    }
}

// Model hiển thị trên UI (Bảng xếp hạng) - Tuyệt đối không cần hp ở đây
public class AchievementDisplayModel
{
    public string playerName { get; set; }
    public int coin { get; set; }
    public float distance { get; set; }
    public string time { get; set; }
}