using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class SceneObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class SaveContainer
{
    public List<SceneObjectData> objects = new List<SceneObjectData>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("--- Tham chiếu UI ---")]
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI heartsText;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject achievementPanel;

    [Header("--- Chỉ số Sinh tồn ---")]
    public int score;
    public int difficulty;
    public int maxHearts = 3;
    public int currentHearts;

    private bool isPaused;
    private bool isGameOver = false;
    private bool isContinueSession = false;

    private Vector3 lastCheckpointPos;
    private bool hasReachedCheckpoint = false;

    void Awake()
    {
        Time.timeScale = 1f;
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        isGameOver = false;
        InfiniteMapGenerator mapGen = FindFirstObjectByType<InfiniteMapGenerator>();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);

        if (player != null)
        {
            lastCheckpointPos = player.position;
            hasReachedCheckpoint = true;
        }

        if (PlayerPrefs.GetInt("IsLoadingSave", 0) == 1)
        {
            PlayerPrefs.SetInt("IsLoadingSave", 0);
            PlayerPrefs.Save();

            Debug.Log("<color=yellow>🔍 [GameManager] Đang chuẩn bị Load dữ liệu từ Database...</color>");
            if (LoadGameFromDatabase())
            {
                isContinueSession = true;
                HandleMapGenerationAfterLoad(mapGen);
                Debug.Log("<color=green><b>✅ [GameManager]</b> Toàn bộ tiến trình đã được phục hồi thành công.</color>");
            }
            else
            {
                Debug.LogWarning("⚠️ [GameManager] Không tìm thấy dữ liệu hợp lệ. Khởi tạo Game mới.");
                StartNewGameLogic(mapGen);
            }
        }
        else
        {
            StartNewGameLogic(mapGen);
        }

        UpdateScoreUI();
        UpdateHeartsUI();
    }

    private void StartNewGameLogic(InfiniteMapGenerator mapGen)
    {
        isContinueSession = false;
        currentHearts = maxHearts;
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        if (mapGen != null) mapGen.InitializeMap(false);
    }

    private void HandleMapGenerationAfterLoad(InfiniteMapGenerator mapGen)
    {
        if (player == null || mapGen == null) return;
        float farthestX = player.position.x;
        SaveableItem[] loadedItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);
        foreach (var item in loadedItems)
        {
            if (item.transform.position.x > farthestX) farthestX = item.transform.position.x;
        }
        mapGen.SetLastX(farthestX);
        mapGen.InitializeMap(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (achievementPanel != null && achievementPanel.activeSelf)
            {
                CloseAchievement();
            }
            else
            {
                if (isPaused) ResumeGame(); else PauseGame();
            }
        }
    }

    // --- QUẢN LÝ UI ---

    public void OpenAchievement()
    {
        if (achievementPanel != null) achievementPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void CloseAchievement()
    {
        if (achievementPanel != null) achievementPanel.SetActive(false);
        if (isGameOver)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
        else if (isPaused)
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }
    }

    // --- LOGIC SINH TỒN ---

    public void TakeDamage()
    {
        if (isGameOver) return;
        currentHearts--;
        UpdateHeartsUI();
        if (currentHearts <= 0) GameOver();
    }

    public void PlayerFall()
    {
        if (isGameOver) return;
        currentHearts = 0;
        UpdateHeartsUI();
        GameOver();
    }

    public void LoadToLastCheckpoint()
    {
        if (player == null) return;

        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        player.position = lastCheckpointPos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);

        UpdateHeartsUI();
        Debug.Log("<color=cyan>🔄 [Respawn] Người chơi đã quay lại Checkpoint.</color>");
    }

    // --- HỆ THỐNG DATABASE (Đã thêm Logs) ---

    public void SaveGameToDatabase()
    {
        if (DatabaseManager.db == null)
        {
            Debug.LogError("❌ [Database] Lỗi: SQLiteConnection chưa được khởi tạo!");
            return;
        }
        if (player == null) return;

        string pName = PlayerPrefs.GetString("playerName", "Player");
        Debug.Log($"<color=cyan>💾 [Database] Đang lưu tiến trình cho: {pName}...</color>");

        SaveContainer container = new SaveContainer();
        SaveableItem[] itemsToSave = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);

        foreach (SaveableItem item in itemsToSave)
        {
            container.objects.Add(new SceneObjectData
            {
                prefabName = item.gameObject.name.Replace("(Clone)", "").Trim(),
                position = item.transform.position,
                rotation = item.transform.rotation
            });
        }

        SaveGameData newSave = new SaveGameData
        {
            playerName = pName,
            difficultyID = PlayerPrefs.GetInt("difficulty", 0),
            score = this.score,
            health = this.currentHearts,
            lives = 0,
            playerPosition = $"{player.position.x}|{player.position.y}|{player.position.z}",
            mapDataJson = JsonUtility.ToJson(container),
            saveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        try
        {
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", newSave.playerName);
            DatabaseManager.db.Insert(newSave);
            Debug.Log($"<color=green>✅ [Database] Lưu thành công! Thời điểm: {newSave.saveDate}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ [Database] Lỗi khi thực thi lệnh SQL: " + e.Message);
        }

        ResumeGame();
    }

    private bool LoadGameFromDatabase()
    {
        if (DatabaseManager.db == null) return false;
        string pName = PlayerPrefs.GetString("playerName", "Player");

        SaveGameData data = DatabaseManager.db.Table<SaveGameData>().Where(s => s.playerName == pName).FirstOrDefault();

        if (data == null)
        {
            Debug.LogWarning($"⚠️ [Database] Không tìm thấy bản lưu nào cho tên: {pName}");
            return false;
        }

        Debug.Log($"<color=yellow>📂 [Database] Đã tìm thấy dữ liệu của {pName}. Đang giải nén map...</color>");

        SaveableItem[] oldItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);
        foreach (SaveableItem item in oldItems) Destroy(item.gameObject);

        this.score = data.score;
        this.currentHearts = data.health;
        this.difficulty = data.difficultyID;

        string[] pos = data.playerPosition.Split('|');
        if (pos.Length == 3 && player != null)
        {
            player.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            SetCheckpoint(player.position);
        }

        SaveContainer container = JsonUtility.FromJson<SaveContainer>(data.mapDataJson);
        if (container != null)
        {
            int count = 0;
            foreach (var item in container.objects)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + item.prefabName);
                if (prefab == null)
                {
                    string[] folders = { "IsLands", "Coin", "enemy", "Platform", "Ground", "Trap", "Hearth", "Decoration", "CheckPoint" };
                    foreach (string f in folders)
                    {
                        prefab = Resources.Load<GameObject>($"Prefabs/{f}/{item.prefabName}");
                        if (prefab != null) break;
                    }
                }
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, item.position, item.rotation);
                    if (obj.GetComponent<SaveableItem>() == null) obj.AddComponent<SaveableItem>();
                    count++;
                }
            }
            Debug.Log($"<color=green>✅ [Database] Tải thành công {count} vật thể từ bản lưu.</color>");
        }
        return true;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (AchievementManager.instance != null) AchievementManager.instance.SaveGameEnd();

        if (isContinueSession && DatabaseManager.db != null)
        {
            string pName = PlayerPrefs.GetString("playerName", "Player");
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
            Debug.Log($"<color=red>🚮 [Database] Đã xóa file lưu của {pName} (Chế độ chơi tiếp - Chết là mất).</color>");
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
    }

    // --- ĐIỀU KHIỂN CHUNG ---

    public void RestartGame()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        LoadingManager.SceneToLoad = "Main Menu";
        SceneManager.LoadScene("LoadingSence");
    }

    public void SetCheckpoint(Vector3 pos) { lastCheckpointPos = pos; hasReachedCheckpoint = true; }
    public void AddHealth(int amount) { currentHearts = Mathf.Min(currentHearts + amount, maxHearts); UpdateHeartsUI(); }
    public void AddScore(int amount) { score += amount; UpdateScoreUI(); }
    private void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + score; }
    private void UpdateHeartsUI() { if (heartsText != null) heartsText.text = "HP: " + currentHearts; }

    public void PauseGame()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);
    }
}