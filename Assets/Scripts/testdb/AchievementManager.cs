using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    private bool isSaved = false; // ❗ chống spam save

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // giữ khi load scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // 💾 SAVE ACHIEVEMENT
    // =========================
    public void SaveAchievement()
    {
        // ❗ tránh save nhiều lần
        if (isSaved) return;

        if (GameManager.instance == null || DatabaseManager.db == null)
        {
            Debug.LogError("❌ Thiếu GameManager hoặc Database!");
            return;
        }

        AchievementData data = new AchievementData();

        data.playerName = PlayerPrefs.GetString("playerName", "Player");

        // 💰 COIN
        data.coin = GameManager.instance.score;

        // 📏 DISTANCE
        if (GameManager.instance.player != null)
            data.distance = GameManager.instance.player.position.x;
        else
            data.distance = 0;

        // ❤️ HP
        data.hp = Mathf.Max(1, GameManager.instance.currentHearts);

        // 🎮 MODE
        data.difficulty = GameManager.instance.difficulty;

        // 🕒 TIME
        data.time = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // 💾 SAVE
        DatabaseManager.db.Insert(data);

        Debug.Log("✅ SAVED | Coin: " + data.coin + " | Distance: " + data.distance);

        isSaved = true;
    }

    // =========================
    // 🏆 LẤY TOP 1 COIN
    // =========================
    public AchievementData GetBestByCoin()
    {
        if (DatabaseManager.db == null)
        {
            Debug.LogError("❌ Database null!");
            return null;
        }

        return DatabaseManager.db
            .Table<AchievementData>()
            .OrderByDescending(x => x.coin)
            .FirstOrDefault();
    }

    // =========================
    // 🏆 LẤY TOP N COIN
    // =========================
    public List<AchievementData> GetTopByCoin(int limit = 5)
    {
        if (DatabaseManager.db == null)
        {
            Debug.LogError("❌ Database null!");
            return new List<AchievementData>();
        }

        return DatabaseManager.db
            .Table<AchievementData>()
            .OrderByDescending(x => x.coin)
            .Take(limit)
            .ToList();
    }

    // =========================
    // 🔄 RESET SAVE FLAG (khi chơi lại)
    // =========================
    public void ResetSave()
    {
        isSaved = false;
    }
}