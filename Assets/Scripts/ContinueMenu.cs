using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueMenu : MonoBehaviour
{
    public GameObject continuePanel;

    public void OpenContinue()
    {
        if (continuePanel != null)
            continuePanel.SetActive(true);
    }

    public void CloseContinue()
    {
        if (continuePanel != null)
            continuePanel.SetActive(false);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
}