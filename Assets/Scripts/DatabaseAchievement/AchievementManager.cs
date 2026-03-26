using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

    // =========================
    // 💾 SAVE ACHIEVEMENT
    // =========================
    public void SaveAchievement()
    {
        Debug.Log(" CALL SAVE | isSaved = " + isSaved);

        // ❗ tránh save nhiều lần
        if (isSaved)
        {
            Debug.Log(" Đã save rồi, bỏ qua!");
            return;
        }

        // ❗ check dependency
        if (GameManager.instance == null)
        {
            Debug.LogError(" GameManager NULL!");
            return;
        }

        if (DatabaseManager.db == null)
        {
            Debug.LogError(" Database NULL!");
            return;
        }

        AchievementData data = new AchievementData();

        try
        {
            // 👤 NAME
            data.playerName = PlayerPrefs.GetString("playerName", "Player");

            // 💰 COIN
            data.coin = GameManager.instance.score;

            // 📏 DISTANCE
            if (GameManager.instance.player != null)
            {
                data.distance = GameManager.instance.player.position.x;
            }
            else
            {
                Debug.LogWarning("⚠️ Player NULL → distance = 0");
                data.distance = 0;
            }

            // ❤️ HP
            data.hp = Mathf.Max(1, GameManager.instance.currentHearts);

            // 🎮 MODE
            data.difficulty = GameManager.instance.difficulty;

            // 🕒 TIME
            data.time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // 💾 INSERT DB
            int result = DatabaseManager.db.Insert(data);

            if (result > 0)
            {
                Debug.Log($" SAVED SUCCESS | Coin: {data.coin} | Distance: {data.distance}");
                isSaved = true;
            }
            else
            {
                Debug.LogError("❌ Insert thất bại!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ SAVE ERROR: " + e.Message);
        }
    }

    // =========================
    // 🏆 TOP 1 COIN
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
    // 🏆 TOP N COIN
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
    // 🔄 RESET SAVE
    // =========================
    public void ResetSave()
    {
        Debug.Log("🔄 Reset Save Flag");
        isSaved = false;
    }

    // =========================
    // 🚀 AUTO RESET KHI LOAD SCENE
    // =========================
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