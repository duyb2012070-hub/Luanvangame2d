using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player")]
    public Transform player;

    [Header("Score")]
    public int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;

    [Header("Game Settings")]
    public int mapSeed;
    public int difficulty;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    bool isPaused;

    [Header("Player Hearts")]
    public int maxHearts = 3;
    public int currentHearts;

    [Header("Checkpoint")]
    Vector3 lastCheckpoint;
    bool hasCheckpoint = false;

    // 👉 chống gọi GameOver nhiều lần
    bool isGameOver = false;

    void Awake()
    {
        Time.timeScale = 1f;

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateScore();

        currentHearts = maxHearts;

        if (player != null)
            lastCheckpoint = player.position;

        difficulty = PlayerPrefs.GetInt("difficulty", 0);

        isGameOver = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // ================= CHECKPOINT =================
    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;

        Debug.Log("Checkpoint Saved: " + pos);
    }

    public void LoadLastCheckpoint()
    {
        if (hasCheckpoint)
        {
            RespawnPlayer();
            Debug.Log("Loaded Checkpoint");
        }
        else
        {
            Debug.Log("No checkpoint!");
        }
    }

    // ================= DAMAGE =================
    public void TakeDamage()
    {
        currentHearts--;

        if (currentHearts <= 0)
        {
            RespawnOrGameOver();
        }
    }

    public void AddHealth(int amount)
    {
        currentHearts += amount;

        if (currentHearts > maxHearts)
            currentHearts = maxHearts;
    }

    public void PlayerFall()
    {
        RespawnOrGameOver();
    }

    void RespawnOrGameOver()
    {
        if (hasCheckpoint)
        {
            RespawnPlayer();
        }
        else
        {
            GameOver();
        }
    }

    public void RespawnPlayer()
    {
        if (player == null) return;

        player.position = lastCheckpoint + new Vector3(0, 2f, 0);
        currentHearts = maxHearts;
    }

    // ================= SCORE =================
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();
    }

    void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    // ================= PAUSE =================
    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // ================= GAME OVER =================
    public void GameOver()
    {
        // ❗ tránh gọi nhiều lần
        if (isGameOver) return;
        isGameOver = true;

        // 💾 SAVE ACHIEVEMENT
        if (AchievementManager.instance != null)
        {
            AchievementManager.instance.SaveAchievement();
            Debug.Log("✅ AUTO SAVED ACHIEVEMENT");
        }
        else
        {
            Debug.LogWarning("❌ Không tìm thấy AchievementManager!");
        }

        // UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score;

        Time.timeScale = 0f;
    }

    // ================= RESTART =================
    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (AchievementManager.instance != null)
            AchievementManager.instance.ResetSave(); // reset cho lượt mới

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToScene(string targetSceneName)
    {
        // 1. Ghi nhớ cảnh muốn đến vào biến tĩnh của LoadingManager
        LoadingManager.SceneToLoad = targetSceneName;

        // 2. Mở Scene Loading lên trước
        SceneManager.LoadScene("LoadingScene");
    }
    // ================= MAIN MENU =================
    public void GoToMainMenu()
    {
        // 1. Đảm bảo thời gian trở lại bình thường (nếu game đang Pause)
        Time.timeScale = 1f;

        // 2. Gán tên Scene muốn đến vào biến tĩnh của LoadingManager
        // Lưu ý: Tên "Main Menu" phải khớp 100% với tên trong Build Settings
        LoadingManager.SceneToLoad = "Main Menu";

        // 3. Load màn hình Loading trước
        // Lưu ý: Tên "LoadingSence" phải khớp với tên bạn đặt (có chữ e ở giữa)
        SceneManager.LoadScene("LoadingSence");
    }
}