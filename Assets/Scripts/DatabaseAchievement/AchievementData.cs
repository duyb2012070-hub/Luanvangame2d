using SQLite;

// 🏆 BẢNG THÀNH TỰU (Đã cập nhật PlayTime)
public class AchievementData
{
    [PrimaryKey, AutoIncrement]
    public int achievementID { get; set; }

    [Indexed]
    public int sessionID { get; set; }

    public int coin { get; set; }
    public float distance { get; set; }

    // GIỮ NGUYÊN để không lỗi các script đang truy cập HP
    public int hp { get; set; }

    // THÊM MỚI: Lưu tổng số giây đã chơi (Dùng để tính Top Time)
    public float playTime { get; set; }

    // Thời điểm thực hiện Save (Ví dụ: "06/04/2026 10:50")
    public string time { get; set; }
}

// 👤 BẢNG THÔNG TIN NGƯỜI CHƠI
public class PlayerData
{
    [PrimaryKey]
    public string playerName { get; set; }
    public string lastPosition { get; set; }
    public int lastHP { get; set; }
    // Có thể thêm playTime ở đây nếu muốn lưu tổng thời gian tích lũy của người chơi
    public float totalPlayTime { get; set; }
}

// 🚩 BẢNG CHECKPOINT
public class CheckpointData
{
    [PrimaryKey, AutoIncrement]
    public int checkpointID { get; set; }

    [Indexed]
    public int sessionID { get; set; }

    public string position { get; set; }
    public string timeReached { get; set; }
}

// ⚙️ BẢNG CÀI ĐẶT
public class SettingsData
{
    [PrimaryKey]
    public string playerName { get; set; }
    public int soundOn { get; set; }
    public int musicOn { get; set; }
    public string resolution { get; set; }
}

// 🎮 BẢNG PHIÊN CHƠI (Bảng trung tâm - Đã cập nhật PlayTime)
public class GameSessionData
{
    [PrimaryKey, AutoIncrement]
    public int sessionID { get; set; }
    public string playerName { get; set; }
    public int difficultyID { get; set; }
    public int score { get; set; }

    // Thời điểm lưu phiên chơi
    public string time { get; set; }

    public float distance { get; set; }
    public int currentHP { get; set; }

    // THÊM MỚI: Thời gian đã trôi qua trong phiên chơi này
    public float playTime { get; set; }
}