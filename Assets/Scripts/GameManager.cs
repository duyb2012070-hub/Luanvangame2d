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
    public int difficulty;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    private bool isPaused;
    private bool isGameOver = false;

    [Header("Player Stats (Hearts)")]
    public int maxHearts = 3;
    public int currentHearts;

    [Header("Lives System (Respawns)")]
    public int totalLives = 3;
    public TextMeshProUGUI livesText; // Kéo Text hiển thị số mạng vào đây

    [Header("Checkpoint")]
    private Vector3 lastCheckpoint;
    private bool hasCheckpoint = false;

    void Awake()
    {
        // Reset thời gian về bình thường mỗi khi load scene
        Time.timeScale = 1f;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        isGameOver = false;
        currentHearts = maxHearts;

        // Lấy độ khó từ Menu gửi qua
        difficulty = PlayerPrefs.GetInt("difficulty", 0);

        if (player != null)
            lastCheckpoint = player.position;

        UpdateScoreUI();
        UpdateLivesUI();
    }

    void Update()
    {
        // Phím tắt Pause game
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // =====================================================
    // ❤️ HỆ THỐNG SINH TỒN & HỒI MÁU (FIX LỖI CS1061)
    // =====================================================

    public void TakeDamage()
    {
        if (isGameOver) return;

        currentHearts--;
        Debug.Log($"[GameManager] Player mất máu! Còn lại: {currentHearts}");

        if (currentHearts <= 0)
        {
            RespawnOrGameOver();
        }
    }

    // Hàm hồi máu cho các vật phẩm HeartCollect gọi tới
    public void AddHealth(int amount)
    {
        currentHearts = Mathf.Min(currentHearts + amount, maxHearts);
        Debug.Log($"[GameManager] Đã hồi {amount} máu. Hiện tại: {currentHearts}");
    }

    // Hàm hồi mạng (Nếu sau này bạn làm vật phẩm tăng Mạng)
    public void AddLife(int amount)
    {
        totalLives += amount;
        UpdateLivesUI();
    }

    // =====================================================
    // 🚩 HỆ THỐNG CHECKPOINT & HỒI SINH
    // =====================================================

    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;
        Debug.Log($"[GameManager] Đã lưu Checkpoint mới tại: {pos}");
    }

    public void PlayerFall()
    {
        if (isGameOver) return;
        RespawnOrGameOver();
    }

    private void RespawnOrGameOver()
    {
        // Nếu còn mạng (Lives) và có Checkpoint -> Cho hồi sinh
        if (hasCheckpoint && totalLives > 0)
        {
            totalLives--; // Trừ 1 mạng
            UpdateLivesUI();
            RespawnPlayer();
        }
        else
        {
            // Hết mạng hoặc chưa chạm checkpoint nào -> Chết thực sự
            GameOver();
        }
    }

    public void RespawnPlayer()
    {
        if (player == null) return;

        // Đưa player về checkpoint và hồi lại đầy Tim cho mạng mới
        player.position = lastCheckpoint + new Vector3(0, 2f, 0);
        currentHearts = maxHearts;
        Debug.Log($"[GameManager] Đã hồi sinh Player. Mạng còn lại: {totalLives}");
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + totalLives;
    }

    // =====================================================
    // 💰 HỆ THỐNG ĐIỂM SỐ
    // =====================================================

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    // =====================================================
    // ⏸️ ĐIỀU KHIỂN & LƯU DATABASE
    // =====================================================

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[GameManager] GAME OVER! Đang thực hiện lưu dữ liệu...");

        // GỌI LƯU DATABASE NGAY KHI THUA
        if (AchievementManager.instance != null)
        {
            AchievementManager.instance.SaveGameEnd();
        }

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + score;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Reset cờ đã lưu để lượt chơi mới có thể lưu tiếp
        if (AchievementManager.instance != null)
            AchievementManager.instance.ResetSave();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        // TỰ ĐỘNG LƯU: Nếu người chơi chủ động thoát ngang từ Menu Pause
        if (!isGameOver && AchievementManager.instance != null)
        {
            AchievementManager.instance.SaveGameEnd();
        }

        LoadingManager.SceneToLoad = "Main Menu";
        SceneManager.LoadScene("LoadingSence");
    }
}