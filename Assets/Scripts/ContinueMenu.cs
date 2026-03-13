using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueMenu : MonoBehaviour
{
    public GameObject continuePanel;

    public void OpenContinue()
    {
        continuePanel.SetActive(true);
    }

    public void CloseContinue()
    {
        continuePanel.SetActive(false);
    }

    public void LoadSlot1()
    {
        GameManager.loadSlot = 0;
        SceneManager.LoadScene("Game");
    }

    public void LoadSlot2()
    {
        GameManager.loadSlot = 1;
        SceneManager.LoadScene("Game");
    }

    public void LoadSlot3()
    {
        GameManager.loadSlot = 2;
        SceneManager.LoadScene("Game");
    }
}