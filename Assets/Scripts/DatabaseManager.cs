using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;

    private SQLiteConnection db;

    void Awake()
    {
        // Singleton để các scene đều truy cập được
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // không bị mất khi đổi scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitDatabase();
    }

    void InitDatabase()
    {
        string dbPath = Application.persistentDataPath + "/game.db";
        db = new SQLiteConnection(dbPath);

        CreateTable();
        InsertDefaultAchievements();
    }

    void CreateTable()
    {
        db.CreateTable<Achievement>();
    }

    void InsertDefaultAchievements()
    {
        if (db.Table<Achievement>().Count() == 0)
        {
            db.Insert(new Achievement { name = "Run 100m", unlocked = false });
            db.Insert(new Achievement { name = "Collect 50 Coins", unlocked = false });
            db.Insert(new Achievement { name = "Finish Level 1", unlocked = false });
        }
    }

    public void UnlockAchievement(string achievementName)
    {
        var achievement = db.Table<Achievement>()
            .Where(x => x.name == achievementName)
            .FirstOrDefault();

        if (achievement != null && achievement.unlocked == false)
        {
            achievement.unlocked = true;
            db.Update(achievement);

            Debug.Log("Achievement unlocked: " + achievementName);
        }
    }


    public List<Achievement> GetAchievements()
    {
        return db.Table<Achievement>().ToList();
    }
}