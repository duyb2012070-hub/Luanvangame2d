using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField nameInput;
    public GameObject playMenuPanel;
    public GameObject difficultyPanel;

    public void OpenPlayMenu()
    {
        playMenuPanel.SetActive(true);
    }

    public void OpenDifficulty()
    {
        playMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    public void BackToPlayMenu()
    {
        difficultyPanel.SetActive(false);
        playMenuPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        playMenuPanel.SetActive(false);
    }

    // EASY
    public void SetEasy()
    {
        DifficultyManager.difficulty = 0;
        SceneManager.LoadScene("game");
    }

    // NORMAL
    public void SetNormal()
    {
        DifficultyManager.difficulty = 1;
        SceneManager.LoadScene("game");
    }

    // HARD
    public void SetHard()
    {
        DifficultyManager.difficulty = 2;
        SceneManager.LoadScene("game");
    }
}