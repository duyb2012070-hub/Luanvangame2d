using UnityEngine;
using SQLite;

public class DatabaseManager : MonoBehaviour
{
    // Singleton instance để các script khác truy cập dễ dàng
    public static DatabaseManager instance;
    public static SQLiteConnection db;

    void Awake()
    {
        // Kiểm tra nếu đã có instance rồi thì xóa cái mới đi, giữ cái cũ (Singleton)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ Database sống xuyên suốt các Scene
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDatabase()
    {
        try
        {
            string path = Application.persistentDataPath + "/game.db";
            db = new SQLiteConnection(path);

            // Tạo các bảng
            db.CreateTable<PlayerData>();
            db.CreateTable<GameSessionData>();
            db.CreateTable<AchievementData>();
            db.CreateTable<SettingsData>();
            db.CreateTable<CheckpointData>();
            db.CreateTable<SaveGameData>();

            Debug.Log("✅ Database initialized successfully at: " + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Database Initialization Error: " + e.Message);
        }
    }

    // Đóng kết nối khi thoát game để tránh lỗi file busy
    private void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("Database connection closed.");
        }
    }
}

// Giữ nguyên định nghĩa Class dữ liệu của bạn
public class SaveGameData
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string playerName { get; set; }
    public int difficultyID { get; set; }
    public int score { get; set; }
    public int health { get; set; }
    public int lives { get; set; }
    public string saveDate { get; set; }
    public string playerPosition { get; set; }
    public string mapDataJson { get; set; }
}