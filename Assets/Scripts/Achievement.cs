using SQLite4Unity3d;

public class Achievement
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }

    public string name { get; set; }

    public bool unlocked { get; set; }
}