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

    [Header("Audio")]
    public AudioSource music;

    // 👉 CHỐNG SAVE LẶP
    bool isSaved = false;

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

        // 👉 LOAD MODE
        difficulty = PlayerPrefs.GetInt("difficulty", 0);

        isSaved = false;
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

    // AUDIO
    public void SetVolume(float volume)
    {
        if (music != null)
            music.volume = volume;
    }

    // CHECKPOINT
    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;

        Debug.Log("Checkpoint Saved: " + pos);
    }

    // DAMAGE
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

    // SCORE
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

    // PAUSE
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

    // GAME OVER (🔥 AUTO SAVE Ở ĐÂY)
    public void GameOver()
    {
        // 👉 chỉ save 1 lần
        if (!isSaved)
        {
            if (AchievementManager.instance != null)
            {
                AchievementManager.instance.SaveAchievement("Player");
                Debug.Log("AUTO SAVED ACHIEVEMENT");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy AchievementManager!");
            }

            isSaved = true;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + score;

        Time.timeScale = 0f;
    }

    // RESTART
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // MAIN MENU
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}