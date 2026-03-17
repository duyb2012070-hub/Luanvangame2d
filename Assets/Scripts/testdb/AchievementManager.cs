using UnityEngine;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void SaveAchievement(string playerName)
    {
        // ❗ kiểm tra an toàn
        if (GameManager.instance == null || DatabaseManager.db == null)
        {
            Debug.LogError("Thiếu GameManager hoặc Database!");
            return;
        }

        AchievementData data = new AchievementData();

        data.playerName = PlayerPrefs.GetString("playerName", "Player");

        // 💰 COIN
        data.coin = GameManager.instance.score;

        // 📏 DISTANCE (chuẩn)
        if (GameManager.instance.player != null)
            data.distance = GameManager.instance.player.position.x;
        else
            data.distance = 0;

        // ❤️ HP (không cho = 0)
        data.hp = Mathf.Max(1, GameManager.instance.currentHearts);

        // 🎮 MODE (QUAN TRỌNG)
        data.difficulty = GameManager.instance.difficulty;

        // 🕒 TIME
        data.time = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // 💾 SAVE
        DatabaseManager.db.Insert(data);

        Debug.Log("✅ AUTO SAVED | Mode: " + data.difficulty +
                  " | Coin: " + data.coin +
                  " | Distance: " + data.distance);
    }
}