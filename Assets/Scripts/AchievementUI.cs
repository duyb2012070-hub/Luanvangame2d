using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AchievementUI : MonoBehaviour
{
    public TextMeshProUGUI achievementText;

    void OnEnable()
    {
        Invoke("ShowAchievements", 0.1f); // đợi database load xong
    }

    public void ShowAchievements()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning("DatabaseManager not ready");
            return;
        }

        List<Achievement> list = DatabaseManager.Instance.GetAchievements();

        string text = "";

        foreach (Achievement a in list)
        {
            string status = a.unlocked ? "Unlocked" : "Locked";
            text += a.name + " - " + status + "\n";
        }

        achievementText.text = text;
    }
}