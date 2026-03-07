using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score")]
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    [Header("Audio")]
    public AudioSource musicSource;

    bool isPaused = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Time.timeScale = 1f;
    }

    void Start()
    {
        UpdateScore();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // Bấm ESC để pause / resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameOverPanel != null && gameOverPanel.activeSelf)
                return;

            if (pausePanel.activeSelf)
                ResumeGame();
            else
                PauseGame();
        }
    }

    //================ SCORE =================

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

    //================ GAME OVER =================

    public void GameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;

        Time.timeScale = 0f;
    }

    //================ PAUSE =================

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

    //================ RESTART =================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //================ MAIN MENU =================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    //================ VOLUME =================

    public void SetVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }
}