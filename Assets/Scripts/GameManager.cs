using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static int loadSlot = -1;

    public Transform player;

    public int score;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText; // ⭐ TEXT HIEN FINAL SCORE

    public int mapSeed;
    public int difficulty;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    bool isPaused;

    // ❤️ PLAYER HEARTS
    public int maxHearts = 3;
    public int currentHearts;

    // 🚩 CHECKPOINT
    Vector3 lastCheckpoint;
    bool hasCheckpoint = false;

    void Awake()
    {
        Time.timeScale = 1f;

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public AudioSource music;

    public void SetVolume(float volume)
    {
        music.volume = volume;
    }

    void Start()
    {
        LoadGame(1);
        UpdateScore();

        currentHearts = maxHearts;

        if (player != null)
            lastCheckpoint = player.position;

        if (loadSlot != -1)
        {
            LoadGame(loadSlot);
            loadSlot = -1;
        }
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

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame(0);
            Debug.Log("GAME SAVED");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame(0);
            Debug.Log("GAME LOADED");
        }
    }

    // 🚩 SET CHECKPOINT

    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;

        Debug.Log("Checkpoint Saved: " + pos);
    }

    // ❤️ DAMAGE FROM TRAP

    public void TakeDamage()
    {
        currentHearts--;

        Debug.Log("Hearts left: " + currentHearts);

        if (currentHearts <= 0)
        {
            RespawnOrGameOver();
        }
    }

    // 🌋 PLAYER FALL

    public void PlayerFall()
    {
        Debug.Log("Player Fell");

        RespawnOrGameOver();
    }

    // 🚩 RESPAWN OR GAME OVER

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

    // 🚩 RESPAWN PLAYER

    public void RespawnPlayer()
    {
        if (player == null) return;

        player.position = lastCheckpoint + new Vector3(0, 2f, 0);

        currentHearts = maxHearts;

        Debug.Log("Respawn at checkpoint");
    }

    // SCORE

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();

        if (score >= 50)
        {
            if (DatabaseManager.Instance != null)
                DatabaseManager.Instance.UnlockAchievement("Collect 50 Coins");
        }

        if (score >= 100)
        {
            if (DatabaseManager.Instance != null)
                DatabaseManager.Instance.UnlockAchievement("Run 100m");
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    // SAVE

    public void SaveGame(int slot)
    {
        SaveData data = new SaveData();

        data.score = score;
        data.mapSeed = mapSeed;
        data.difficulty = difficulty;

        if (player != null)
        {
            data.posX = player.position.x;
            data.posY = player.position.y;
            data.posZ = player.position.z;
        }

        SaveSystem.SaveGame(data, slot);
    }

    // LOAD

    public void LoadGame(int slot)
    {
        SaveData data = SaveSystem.LoadGame(slot);

        if (data != null)
        {
            score = data.score;
            mapSeed = data.mapSeed;
            difficulty = data.difficulty;

            if (player != null)
            {
                player.position = new Vector3(
                    data.posX,
                    data.posY,
                    data.posZ
                );

                lastCheckpoint = player.position;
            }

            UpdateScore();
        }
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

    // GAME OVER

    public void GameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // ⭐ HIEN FINAL SCORE
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