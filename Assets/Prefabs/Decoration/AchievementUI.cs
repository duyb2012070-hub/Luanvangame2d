using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform content;       // Scroll view content
    public GameObject itemPrefab;   // Prefab hiển thị 1 achievement

    [Header("Mode Buttons")]
    public GameObject easyButton;
    public GameObject normalButton;
    public GameObject hardButton;

    void Start()
    {
        // Khi mở panel lần đầu, content trống
        ClearContent();

        // Gắn sự kiện cho 3 nút chế độ
        if (easyButton != null)
            easyButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowTopByMode(0));

        if (normalButton != null)
            normalButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowTopByMode(1));

        if (hardButton != null)
            hardButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ShowTopByMode(2));
    }

    // Xóa toàn bộ item cũ
    public void ClearContent()
    {
        if (content == null) return;
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    // Hiển thị Top 10 theo chế độ: 0 = Easy, 1 = Normal, 2 = Hard
    public void ShowTopByMode(int mode)
    {
        if (content == null || itemPrefab == null)
        {
            Debug.LogError("Chưa gán content hoặc itemPrefab!");
            return;
        }

        ClearContent();

        // Lấy dữ liệu top 10 coin theo chế độ
        List<AchievementData> list = DatabaseManager.db
            .Table<AchievementData>()
            .Where(x => x.difficulty == mode)
            .OrderByDescending(x => x.coin)
            .Take(10)
            .ToList();

        if (list.Count == 0)
        {
            // Nếu chưa có dữ liệu
            GameObject item = Instantiate(itemPrefab, content);
            TMP_Text txt = item.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = "Chưa có dữ liệu!";
            return;
        }

        int rank = 1;
        foreach (var data in list)
        {
            GameObject item = Instantiate(itemPrefab, content);
            TMP_Text txt = item.GetComponentInChildren<TMP_Text>();
            if (txt == null)
            {
                Debug.LogError("Prefab thiếu TMP_Text!");
                continue;
            }

            string modeText = mode == 0 ? "Easy" : mode == 1 ? "Normal" : "Hard";

            txt.text =
                " TOP " + rank + "\n" +
                "NAME: " + data.playerName + "\n" +
                "COIN: " + data.coin + "\n" +
                "DISTANCE: " + data.distance.ToString("F1") + "\n" +
                "HP: " + data.hp + "\n" +
                "MODE: " + modeText + "\n" +
                "TIME: " + data.time;

       

            rank++;
        }
    }
}