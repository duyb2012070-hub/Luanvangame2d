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
    public int mapSeed;
    public int difficulty;

    public GameObject pausePanel;
    public GameObject gameOverPanel;

    bool isPaused;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadGame(1);
        UpdateScore();

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

    // SCORE

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();

        // ===== ACHIEVEMENT CHECK =====

        if (score >= 50)
        {
            if (DatabaseManager.Instance != null)
            {
                DatabaseManager.Instance.UnlockAchievement("Collect 50 Coins");
            }
        }

        if (score >= 100)
        {
            if (DatabaseManager.Instance != null)
            {
                DatabaseManager.Instance.UnlockAchievement("Run 100m");
            }
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
            }
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