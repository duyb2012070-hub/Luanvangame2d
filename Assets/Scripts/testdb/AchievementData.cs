using SQLite;

public class AchievementData
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    public string playerName { get; set; }

    public int coin { get; set; }

    public float distance { get; set; }

    public int hp { get; set; }

    public int difficulty { get; set; }

    public string time { get; set; }
}