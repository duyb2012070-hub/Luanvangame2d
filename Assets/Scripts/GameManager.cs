using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class SceneObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class SaveContainer { public List<SceneObjectData> objects = new List<SceneObjectData>(); }

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

    [Header("--- Cấu hình Âm thanh (Audio) ---")]
    public AudioSource backgroundMusic;
    public List<AudioSource> sfxSources = new List<AudioSource>();
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("--- Chỉ số Sinh tồn ---")]
    public int score;
    public int difficulty;
    public int maxHearts = 3;
    public int currentHearts;

    [Header("--- Hệ thống Thời gian ---")]
    public float playTimer;

    [Header("--- Hệ số Quãng đường ---")]
    private float startPosX;
    private bool isPaused;
    private bool isGameOver = false;
    private bool isContinueSession = false;

    private Vector3 lastCheckpointPos;
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
        InfiniteMapGenerator mapGen = FindFirstObjectByType<InfiniteMapGenerator>();

        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (multiplierText) multiplierText.gameObject.SetActive(false);

        if (player != null)
        {
            lastCheckpointPos = player.position;
            startPosX = player.position.x;
        }

        InitVolumeSettings();

        // KIỂM TRA TRẠNG THÁI LOAD GAME
        if (PlayerPrefs.GetInt("IsLoadingSave", 0) == 1)
        {
            PlayerPrefs.SetInt("IsLoadingSave", 0);
            PlayerPrefs.Save();

            if (LoadGameFromDatabase())
            {
                isContinueSession = true;
                HandleMapGenerationAfterLoad(mapGen);
                Debug.Log("<color=green>✅ Game Loaded Successfully!</color>");
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
        playTimer = 0f;
        difficulty = PlayerPrefs.GetInt("difficulty", 0);
        if (mapGen != null) mapGen.InitializeMap(false);
    }
    #endregion

    #region SQLITE (SAVE & LOAD)
    public void SaveGameToDatabase()
    {
        if (DatabaseManager.db == null || player == null) return;
        string pName = PlayerPrefs.GetString("playerName", "Player");

        SaveContainer container = new SaveContainer();
        // Chỉ lưu những vật thể được đánh dấu là SaveableItem
        SaveableItem[] itemsToSave = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);

        foreach (SaveableItem item in itemsToSave)
        {
            container.objects.Add(new SceneObjectData
            {
                // Xóa "(Clone)" để khi Load lại có thể tìm đúng tên file trong Resources
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
            playTime = this.playTimer,
            playerPosition = $"{player.position.x}|{player.position.y}|{player.position.z}",
            mapDataJson = JsonUtility.ToJson(container),
            saveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")
        };

        // Ghi đè dữ liệu cũ của cùng một Player
        DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
        DatabaseManager.db.Insert(newSave);

        Debug.Log("<color=blue>💾 Game Saved to SQLite!</color>");
        ResumeGame();
    }

    private bool LoadGameFromDatabase()
    {
        if (DatabaseManager.db == null) return false;
        string pName = PlayerPrefs.GetString("playerName", "Player");
        SaveGameData data = DatabaseManager.db.Table<SaveGameData>().Where(s => s.playerName == pName).FirstOrDefault();

        if (data == null) return false;

        // --- BƯỚC QUAN TRỌNG: XÓA CÁC ĐỐI TƯỢNG CŨ TRƯỚC KHI NẠP ---
        SaveableItem[] oldItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);
        foreach (var item in oldItems) Destroy(item.gameObject);

        // Nạp chỉ số cơ bản
        this.score = data.score;
        this.currentHearts = data.health;
        this.playTimer = data.playTime;

        // Nạp vị trí người chơi
        if (player != null)
        {
            string[] pos = data.playerPosition.Split('|');
            player.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            SetCheckpoint(player.position);
        }

        // Nạp địa hình, quái, vật phẩm từ JSON
        SaveContainer container = JsonUtility.FromJson<SaveContainer>(data.mapDataJson);
        if (container != null)
        {
            foreach (var item in container.objects)
            {
                // Tìm Prefab trong tất cả các folder con của Resources/Prefabs
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
                    GameObject newObj = Instantiate(prefab, item.position, item.rotation);
                    newObj.name = item.prefabName; // Gán lại tên chuẩn

                    // Đảm bảo có script đánh dấu để có thể save tiếp ở lần sau
                    if (newObj.GetComponent<SaveableItem>() == null)
                        newObj.AddComponent<SaveableItem>();
                }
            }
        }
        return true;
    }

    private void HandleMapGenerationAfterLoad(InfiniteMapGenerator mapGen)
    {
        if (player == null || mapGen == null) return;

        // Tìm điểm xa nhất hiện có trên map vừa load để Generator xây tiếp từ đó
        float farthestX = player.position.x;
        SaveableItem[] allItems = GameObject.FindObjectsByType<SaveableItem>((FindObjectsSortMode)0);

        foreach (var item in allItems)
        {
            if (item.transform.position.x > farthestX) farthestX = item.transform.position.x;
        }

        mapGen.SetLastX(farthestX);
        mapGen.InitializeMap(true); // Gọi chế độ Initialize cho Load Save
    }
    #endregion

    #region SINH TỒN & ĐIỂM SỐ
    public void AddScore(int amount)
    {
        int multiplier = GetCurrentMultiplier();
        score += amount * multiplier;
        UpdateScoreUI();
        if (multiplier > 1) ShowAndHideMultiplier(multiplier);
    }

    public void AddHealth(int amount) { currentHearts = Mathf.Min(currentHearts + amount, maxHearts); UpdateHeartsUI(); }

    public void TakeDamage()
    {
        if (isGameOver) return;
        currentHearts--;
        UpdateHeartsUI();
        if (currentHearts <= 0) GameOver();
    }

    public void PlayerFall() { if (isGameOver) return; currentHearts = 0; UpdateHeartsUI(); GameOver(); }

    public int GetCurrentMultiplier()
    {
        if (player == null) return 1;
        float distance = player.position.x - startPosX;
        if (distance > 500) return 4;
        if (distance > 300) return 3;
        if (distance > 100) return 2;
        return 1;
    }

    private void ShowAndHideMultiplier(int mult)
    {
        if (multiplierText == null) return;
        CancelInvoke(nameof(DisableMultiplierUI));
        multiplierText.text = "X" + mult;
        multiplierText.gameObject.SetActive(true);
        Invoke(nameof(DisableMultiplierUI), 1.0f);
    }
    private void DisableMultiplierUI() => multiplierText.gameObject.SetActive(false);
    #endregion

    #region QUẢN LÝ UI & TRẠNG THÁI SCENE
    public void ResumeGame() { isPaused = false; Time.timeScale = 1f; if (pausePanel) pausePanel.SetActive(false); }
    public void PauseGame() { if (isGameOver) return; isPaused = true; Time.timeScale = 0f; if (pausePanel) pausePanel.SetActive(true); }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (AchievementManager.instance != null) AchievementManager.instance.SaveGameEnd();

        // Nếu người chơi chết, xóa dữ liệu save cũ để bắt đầu lại từ đầu ở phiên sau
        if (DatabaseManager.db != null)
        {
            string pName = PlayerPrefs.GetString("playerName", "Player");
            DatabaseManager.db.Execute("DELETE FROM SaveGameData WHERE playerName = ?", pName);
        }

        Time.timeScale = 0f;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreText) finalScoreText.text = "Final Score: " + score;
    }

    public void SetCheckpoint(Vector3 pos) { lastCheckpointPos = pos; }

    public void UpdateScoreUI() { if (scoreText) scoreText.text = "Score: " + score; }
    public void UpdateHeartsUI() { if (heartsText) heartsText.text = "HP: " + currentHearts; }
    public void RestartGame() { PlayerPrefs.SetInt("IsLoadingSave", 0); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("Main Menu"); }
    #endregion

    #region ÂM THANH (AUDIO)
    private void InitVolumeSettings()
    {
        float mVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sVol = PlayerPrefs.GetFloat("SfxVolume", 0.5f);

        if (musicSlider != null)
        {
            musicSlider.value = mVol;
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sVol;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        ApplyMusicVolume(mVol);
        ApplySfxVolume(sVol);
    }

    public void OnMusicSliderChanged(float value) { ApplyMusicVolume(value); PlayerPrefs.SetFloat("MusicVolume", value); }
    public void OnSfxSliderChanged(float value) { ApplySfxVolume(value); PlayerPrefs.SetFloat("SfxVolume", value); }
    private void ApplyMusicVolume(float vol) { if (backgroundMusic != null) backgroundMusic.volume = vol; }
    private void ApplySfxVolume(float vol) { foreach (var s in sfxSources) if (s != null) s.volume = vol; }
    #endregion

    void Update()
    {
        if (isGameOver) return;
        if (!isPaused) playTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
}