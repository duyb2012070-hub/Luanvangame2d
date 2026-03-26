using UnityEngine;
using SQLite;

public class DatabaseManager : MonoBehaviour
{
    public static SQLiteConnection db;

    void Awake()
    {
        string path = Application.persistentDataPath + "/game.db";
        db = new SQLiteConnection(path);

        // Tạo tất cả các bảng theo sơ đồ ERD
        db.CreateTable<PlayerData>();
        db.CreateTable<SettingsData>();
        db.CreateTable<GameSessionData>();
        db.CreateTable<AchievementData>();
        db.CreateTable<CheckpointData>(); // Nếu bạn tạo class CheckpointData tương tự

        Debug.Log("Database initialized at: " + path);
    }
}