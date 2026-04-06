using UnityEngine;
using SQLite;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    public static SQLiteConnection db;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
            // Sử dụng Path.Combine để tránh lỗi gạch chéo ngược/xuôi trên các hệ điều hành khác nhau
            string path = Path.Combine(Application.persistentDataPath, "game.db");

            // Mở kết nối
            db = new SQLiteConnection(path);

            // Tạo các bảng nếu chưa tồn tại (Nếu bảng đã có, SQLite sẽ tự bỏ qua, không mất dữ liệu cũ)
            db.CreateTable<PlayerData>();
            db.CreateTable<GameSessionData>();
            db.CreateTable<AchievementData>();
            db.CreateTable<SettingsData>();
            db.CreateTable<CheckpointData>();
            db.CreateTable<SaveGameData>();

            Debug.Log($"<color=green>✅ Database initialized successfully at:</color> {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Database Initialization Error: " + e.Message);
        }
    }

    // Hàm tiện ích: Xóa toàn bộ Save để test mới hoàn toàn (Dùng khi cần)
    public void ResetAllData()
    {
        if (db != null)
        {
            db.DeleteAll<SaveGameData>();
            db.DeleteAll<AchievementData>();
            Debug.Log("⚠️ All database tables cleared.");
        }
    }

    private void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("Database connection closed.");
        }
    }
}

// Model SaveGameData chuẩn hóa để khớp với GameManager của bạn
public class SaveGameData
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    [Indexed] // Index giúp tìm kiếm tên người chơi nhanh hơn khi Load
    public string playerName { get; set; }

    public int difficultyID { get; set; }
    public int score { get; set; }
    public int health { get; set; } // GIỮ NGUYÊN để không lỗi script khác
    public int lives { get; set; }

    // Thời điểm bấm lưu (Ví dụ: "06/04/2026 10:30")
    public string saveDate { get; set; }

    // TỔNG THỜI GIAN ĐÃ CHƠI (Tính bằng giây, ví dụ: 120.5f)
    // Thêm mới theo yêu cầu của bạn
    public float playTime { get; set; }

    public string playerPosition { get; set; }
    public string mapDataJson { get; set; }
}