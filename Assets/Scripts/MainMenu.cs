using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField nameInput;

    public GameObject mainMenuPanel;
    public GameObject playMenuPanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject guidePanel;
    public GameObject achievementPanel; // THÊM PANEL ACHIEVEMENT

    void Start()
    {
        ShowMainMenu();
    }

    // Ẩn tất cả panel
    void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        playMenuPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        settingsPanel.SetActive(false);
        guidePanel.SetActive(false);
        achievementPanel.SetActive(false); // thêm
    }

    // Hiện menu chính
    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    // Mở menu Play
    public void OpenPlayMenu()
    {
        HideAllPanels();
        playMenuPanel.SetActive(true);
    }

    // Mở menu chọn độ khó
    public void OpenDifficulty()
    {
        HideAllPanels();
        difficultyPanel.SetActive(true);
    }

    // Mở menu Settings
    public void OpenSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    // MỞ GUIDE
    public void OpenGuide()
    {
        HideAllPanels();
        guidePanel.SetActive(true);
    }

    // ĐÓNG GUIDE
    public void CloseGuide()
    {
        ShowMainMenu();
    }

    // MỞ ACHIEVEMENT
    public void OpenAchievement()
    {
        HideAllPanels();
        achievementPanel.SetActive(true);
    }

    // ĐÓNG ACHIEVEMENT
    public void CloseAchievement()
    {
        ShowMainMenu();
    }

    // Back về Play Menu
    public void BackToPlayMenu()
    {
        HideAllPanels();
        playMenuPanel.SetActive(true);
    }

    // Back về Main Menu
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    // Reset tên người chơi
    public void ResetName()
    {
        nameInput.text = "";
    }

    // Bật tắt fullscreen
    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Thay đổi âm lượng
    public void ChangeSFXVolume(float value)
    {
        AudioListener.volume = value;
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Chọn độ khó Easy
    public void SetEasy()
    {
        DifficultyManager.difficulty = 0;
        SceneManager.LoadScene("game");
    }

    // Chọn độ khó Normal
    public void SetNormal()
    {
        DifficultyManager.difficulty = 1;
        SceneManager.LoadScene("game");
    }

    // Chọn độ khó Hard
    public void SetHard()
    {
        DifficultyManager.difficulty = 2;
        SceneManager.LoadScene("game");
    }
    //================ NEW GAME =================

    public void NewGameEasy()
    {
        CreateNewGame(0);
    }

    public void NewGameNormal()
    {
        CreateNewGame(1);
    }

    public void NewGameHard()
    {
        CreateNewGame(2);
    }

    void CreateNewGame(int difficulty)
    {
        SaveData data = new SaveData();

        data.score = 0;
        data.difficulty = difficulty;

        data.posX = 0;
        data.posY = 2;
        data.posZ = 0;

        SaveSystem.SaveGame(data, 1);

        Debug.Log("New Game Created - Difficulty: " + difficulty);

        SceneManager.LoadScene("game");
    }
    //================ CONTINUE =================

    public void ContinueGame()
    {
        if (SaveSystem.HasSave(1))
        {
            SceneManager.LoadScene("game");
        }
        else
        {
            Debug.Log("No Save File Found");
        }
    }
}