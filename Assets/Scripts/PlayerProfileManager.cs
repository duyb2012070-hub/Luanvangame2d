using UnityEngine;
using TMPro;

public class PlayerProfileManager : MonoBehaviour
{
    public TMP_InputField nameInput;

    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SaveName()
    {
        PlayerPrefs.SetString("PlayerName", nameInput.text);
        PlayerPrefs.Save();
    }
}