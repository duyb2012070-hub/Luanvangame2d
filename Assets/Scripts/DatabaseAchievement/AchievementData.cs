using SQLite;

// 🏆 BẢNG THÀNH TỰU (Lưu chỉ số kỷ lục)
public class AchievementData
{
    [PrimaryKey, AutoIncrement]
    public int achievementID { get; set; }

    [Indexed]
    public int sessionID { get; set; } // Liên kết với Session

    public int coin { get; set; }
    public float distance { get; set; }
    public int hp { get; set; }

    // ĐÃ SỬA: Chuyển float -> string để lưu "HH:mm:ss" không bị lỗi convert
    public string time { get; set; }
}

// 👤 BẢNG THÔNG TIN NGƯỜI CHƠI (Lưu trạng thái cuối cùng)
public class PlayerData
{
    [PrimaryKey]
    public string playerName { get; set; }
    public string lastPosition { get; set; } // Lưu dạng "x,y,z"
    public int lastHP { get; set; }
}

// 🚩 BẢNG CHECKPOINT (Lưu các điểm đã đi qua)
public class CheckpointData
{
    [PrimaryKey, AutoIncrement]
    public int checkpointID { get; set; }

    [Indexed]
    public int sessionID { get; set; }

    public string position { get; set; } // "x,y,z"
    public string timeReached { get; set; }
}

// ⚙️ BẢNG CÀI ĐẶT (Lưu cấu hình theo từng người chơi)
public class SettingsData
{
    [PrimaryKey]
    public string playerName { get; set; }
    public int soundOn { get; set; }
    public int musicOn { get; set; }
    public string resolution { get; set; }
}

// 🎮 BẢNG PHIÊN CHƠI (Bảng trung tâm liên kết mọi thứ)
public class GameSessionData
{
    [PrimaryKey, AutoIncrement]
    public int sessionID { get; set; }
    public string playerName { get; set; }
    public int difficultyID { get; set; }
    public int score { get; set; }

    // ĐÃ SỬA: Chuyển float -> string để đồng bộ với AchievementData
    public string time { get; set; }

    public float distance { get; set; }
    public int currentHP { get; set; }
}
