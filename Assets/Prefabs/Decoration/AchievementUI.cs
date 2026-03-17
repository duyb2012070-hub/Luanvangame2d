using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AchievementUI : MonoBehaviour
{
    public Transform content;       // nơi chứa item
    public GameObject itemPrefab;   // prefab UI

    public void ShowLast10()
    {
        // ❗ Check null để tránh crash
        if (content == null || itemPrefab == null)
        {
            Debug.LogError("Chưa gán content hoặc itemPrefab!");
            return;
        }

        // ❌ Xóa item cũ
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 📥 Lấy 10 data mới nhất
        List<AchievementData> list = DatabaseManager.db
            .Table<AchievementData>()
            .OrderByDescending(x => x.id)
            .Take(10)
            .ToList();

        // 📤 Hiển thị
        foreach (var data in list)
        {
            GameObject item = Instantiate(itemPrefab, content);

            // ✅ LẤY TEXT ĐÚNG (QUAN TRỌNG)
            TMP_Text txt = item.GetComponentInChildren<TMP_Text>();

            if (txt == null)
            {
                Debug.LogError("Prefab thiếu TMP_Text!");
                continue;
            }

            string mode = "Easy";
            if (data.difficulty == 1) mode = "Normal";
            if (data.difficulty == 2) mode = "Hard";

            txt.text =
                "NAME: " + data.playerName + "\n" +
                "COIN: " + data.coin + "\n" +
                "DISTANCE: " + data.distance.ToString("F1") + "\n" +
                "HP: " + data.hp + "\n" +
                "Mode: " + mode + "\n" +
                "TIME: " + data.time;
        }
    }
}