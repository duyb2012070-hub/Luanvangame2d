using UnityEngine;
using SQLite;

public class DatabaseManager : MonoBehaviour
{
    public static SQLiteConnection db;

    void Awake()
    {
        string path = Application.persistentDataPath + "/game.db";

        db = new SQLiteConnection(path);

        db.CreateTable<AchievementData>();

        Debug.Log("DB OK: " + path);
    }
}