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

    [Header("--- Chỉ số Sinh tồn ---")]
    public int score;
    public int difficulty;
    public int maxHearts = 3;
    public int currentHearts;

    private bool isPaused;
    private bool isGameOver = false;
    private bool isContinueSession = false; // Xác định phiên chơi này được load từ save

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

        // Kiểm tra nếu người chơi bấm "Continue" từ Menu
        if (PlayerPrefs.GetInt("IsLoadingSave", 0) == 1)
        {
            // Reset cờ ngay lập tức để tránh việc tự động load sai lệch lần sau
            PlayerPrefs.SetInt("IsLoadingSave", 0);
            PlayerPrefs.Save();

            bool loadSuccess = LoadGameFromDatabase();

            if (loadSuccess)
            {
                isContinueSession = true;
                HandleMapGenerationAfterLoad(mapGen);
                Debug.Log("📂 [GameManager] Load dữ liệu thành công.");
            }
            else
            {
                Debug.LogWarning("⚠️ [GameManager] Không tìm thấy dữ liệu trong Database. Bắt đầu phiên chơi mới.");
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
        hasReachedCheckpoint = false;
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
            if (isPaused) ResumeGame(); else PauseGame();
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

    // --- XỬ LÝ DATABASE ---

    public void SaveGameToDatabase()
    {
        if (DatabaseManager.db == null || player == null) return;

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
            playerName = PlayerPrefs.GetString("playerName", "Player"),
            difficultyID = PlayerPrefs.GetInt("difficulty", 0),
            score = this.score,
            health = this.currentHearts,
            lives = 0,
            playerPosition = $"{player.position.x}|{player.position.y}|{player.position.z}",
            mapDataJson = JsonUtility.ToJson(container),
            saveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", newSave.playerName);
        DatabaseManager.db.Insert(newSave);
        Debug.Log("✅ [Database] Đã lưu tiến trình.");
        ResumeGame();
    }

    private bool LoadGameFromDatabase()
    {
        if (DatabaseManager.db == null) return false;
        string pName = PlayerPrefs.GetString("playerName", "Player");
        SaveGameData data = DatabaseManager.db.Table<SaveGameData>().Where(s => s.playerName == pName).FirstOrDefault();

        if (data == null) return false;

        // Xóa các vật thể cũ hiện có trên Scene trước khi Instantiate
        SaveableItem[] oldItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);
        foreach (SaveableItem item in oldItems) Destroy(item.gameObject);

        this.score = data.score;
        this.currentHearts = data.health;
        this.difficulty = data.difficultyID;

        // Load vị trí Player
        string[] pos = data.playerPosition.Split('|');
        if (pos.Length == 3 && player != null)
        {
            player.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            SetCheckpoint(player.position);
        }

        // Load Map Objects
        SaveContainer container = JsonUtility.FromJson<SaveContainer>(data.mapDataJson);
        string[] folders = { "IsLands", "Coin", "enemy", "Platform", "Ground", "Trap", "Hearth", "Decoration", "CheckPoint" };

        if (container != null)
        {
            foreach (var item in container.objects)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/" + item.prefabName);
                if (prefab == null)
                {
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
                }
            }
        }
        return true;
    }

    private void DeleteSaveDataOnDeath()
    {
        // Chỉ xóa data nếu phiên chơi hiện tại là từ một file Save (Continue)
        if (isContinueSession && DatabaseManager.db != null)
        {
            string pName = PlayerPrefs.GetString("playerName", "Player");
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
            Debug.Log($"🚮 [Database] Đã xóa file lưu của {pName} vì chết trong phiên Continue.");
        }
    }

    // --- UI & HỆ THỐNG ---

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (AchievementManager.instance != null) AchievementManager.instance.SaveGameEnd();

        DeleteSaveDataOnDeath();

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
    }

    public void RestartGame()
    {
        PlayerPrefs.SetInt("IsLoadingSave", 0);
        PlayerPrefs.Save();
        if (AchievementManager.instance != null) AchievementManager.instance.ResetSave();
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
    public void PauseGame() { if (isGameOver) return; isPaused = true; Time.timeScale = 0f; if (pausePanel != null) pausePanel.SetActive(true); }
    public void ResumeGame() { isPaused = false; Time.timeScale = 1f; if (pausePanel != null) pausePanel.SetActive(false); }
}