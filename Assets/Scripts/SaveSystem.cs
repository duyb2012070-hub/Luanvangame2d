using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string GetSavePath(int slot)
    {
        return Application.persistentDataPath + "/save_" + slot + ".json";
    }

    public static void SaveGame(SaveData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
    }

    public static SaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        return null;
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }
}