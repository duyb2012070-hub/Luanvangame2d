using UnityEngine;
using UnityEngine.UI;

public class UIHearts : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Update()
    {
        int currentHearts = GameManager.instance.currentHearts;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHearts)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }
}