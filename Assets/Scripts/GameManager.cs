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

    #region BIẾN THAM CHIẾU VÀ CHỈ SỐ
    [Header("--- Tham chiếu UI ---")]
    public Transform player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI heartsText;
    public TextMeshProUGUI multiplierText;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject achievementPanel;

    [Header("--- Chỉ số Sinh tồn ---")]
    public int score;
    public int difficulty;
    public int maxHearts = 3;
    public int currentHearts;

    [Header("--- Hệ thống Thời gian ---")]
    public float playTimer; // Biến đếm tổng thời gian chơi (giây)

    [Header("--- Hệ số Quãng đường ---")]
    private float startPosX;
    private bool isPaused;
    private bool isGameOver = false;
    private bool isContinueSession = false;

    private Vector3 lastCheckpointPos;
    private bool hasReachedCheckpoint;
    #endregion

    #region KHỞI TẠO HỆ THỐNG
    void Awake()
    {
        Time.timeScale = 1f;
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        isGameOver = false;
        playTimer = 0f; // Mặc định là 0
        InfiniteMapGenerator mapGen = FindFirstObjectByType<InfiniteMapGenerator>();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);
        if (multiplierText != null) multiplierText.gameObject.SetActive(false);

        if (player != null)
        {
            lastCheckpointPos = player.position;
            startPosX = player.position.x;
            hasReachedCheckpoint = true;
        }

        if (PlayerPrefs.GetInt("IsLoadingSave", 0) == 1)
        {
            PlayerPrefs.SetInt("IsLoadingSave", 0);
            PlayerPrefs.Save();

            if (LoadGameFromDatabase())
            {
                isContinueSession = true;
                HandleMapGenerationAfterLoad(mapGen);
            }
            else StartNewGameLogic(mapGen);
        }
        else StartNewGameLogic(mapGen);

        UpdateScoreUI();
        UpdateHeartsUI();
    }

    private void StartNewGameLogic(InfiniteMapGenerator mapGen)
    {
        isContinueSession = false;
        currentHearts = maxHearts;
        playTimer = 0f; // Game mới bắt đầu từ 0 giây
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
    #endregion

    #region VÒNG LẶP CẬP NHẬT
    void Update()
    {
        if (isGameOver || isPaused) return;

        // Cập nhật bộ đếm thời gian mỗi giây
        playTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (achievementPanel != null && achievementPanel.activeSelf) CloseAchievement();
            else if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
    #endregion

    #region HỆ THỐNG ĐIỂM SỐ & MULTIPLIER
    public int GetCurrentMultiplier()
    {
        if (player == null) return 1;
        float distance = player.position.x - startPosX;
        if (distance > 500) return 4;
        if (distance > 300) return 3;
        if (distance > 100) return 2;
        return 1;
    }

    public void AddScore(int amount)
    {
        int multiplier = GetCurrentMultiplier();
        score += amount * multiplier;
        UpdateScoreUI();
        if (multiplier > 1) ShowAndHideMultiplier(multiplier);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score " + score;
    }

    private void ShowAndHideMultiplier(int mult)
    {
        if (multiplierText == null) return;
        CancelInvoke(nameof(DisableMultiplierUI));
        multiplierText.text = "X" + mult;
        multiplierText.gameObject.SetActive(true);
        Invoke(nameof(DisableMultiplierUI), 1.0f);
    }

    private void DisableMultiplierUI()
    {
        if (multiplierText != null) multiplierText.gameObject.SetActive(false);
    }
    #endregion

    #region SINH TỒN & DATABASE
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

    public void AddHealth(int amount)
    {
        currentHearts = Mathf.Min(currentHearts + amount, maxHearts);
        UpdateHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        if (heartsText != null) heartsText.text = "HP: " + currentHearts;
    }

    public void SetCheckpoint(Vector3 pos) { lastCheckpointPos = pos; hasReachedCheckpoint = true; }

    public void SaveGameToDatabase()
    {
        if (DatabaseManager.db == null || player == null) return;
        string pName = PlayerPrefs.GetString("playerName", "Player");

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
            playTime = this.playTimer, // LƯU THỜI GIAN VÀO DB TẠI ĐÂY
            playerPosition = $"{player.position.x}|{player.position.y}|{player.position.z}",
            mapDataJson = JsonUtility.ToJson(container),
            saveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
        DatabaseManager.db.Insert(newSave);
        ResumeGame();
    }

    private bool LoadGameFromDatabase()
    {
        if (DatabaseManager.db == null) return false;
        string pName = PlayerPrefs.GetString("playerName", "Player");
        SaveGameData data = DatabaseManager.db.Table<SaveGameData>().Where(s => s.playerName == pName).FirstOrDefault();

        if (data == null) return false;

        SaveableItem[] oldItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);
        foreach (SaveableItem item in oldItems) Destroy(item.gameObject);

        this.score = data.score;
        this.currentHearts = data.health;
        this.difficulty = data.difficultyID;
        this.playTimer = data.playTime; // TẢI THỜI GIAN ĐÃ CHƠI TRƯỚC ĐÓ ĐỂ CHẠY TIẾP

        string[] pos = data.playerPosition.Split('|');
        if (pos.Length == 3 && player != null)
        {
            player.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            SetCheckpoint(player.position);
        }

        SaveContainer container = JsonUtility.FromJson<SaveContainer>(data.mapDataJson);
        if (container != null)
        {
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
                }
            }
        }
        return true;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Lưu vào bảng xếp hạng (Achievement)
        if (AchievementManager.instance != null)
            AchievementManager.instance.SaveGameEnd();

        if (isContinueSession && DatabaseManager.db != null)
        {
            string pName = PlayerPrefs.GetString("playerName", "Player");
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
        }
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
    }

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

    public void OpenAchievement() { if (achievementPanel != null) achievementPanel.SetActive(true); if (pausePanel != null) pausePanel.SetActive(false); }
    public void CloseAchievement() { if (achievementPanel != null) achievementPanel.SetActive(false); if (isGameOver) gameOverPanel.SetActive(true); else if (isPaused) pausePanel.SetActive(true); }
    public void RestartGame() { PlayerPrefs.SetInt("IsLoadingSave", 0); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("Main Menu"); }
    #endregion
}