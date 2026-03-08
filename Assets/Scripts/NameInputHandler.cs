using TMPro;
using UnityEngine;

public class NameInputHandler : MonoBehaviour
{
    public TMP_InputField nameInput;

    public void SaveName()
    {
        string playerName = nameInput.text;

        if (playerName == "")
        {
            playerName = "Player";
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        Debug.Log("Saved Name: " + playerName);
    }
}