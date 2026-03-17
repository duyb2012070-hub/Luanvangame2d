using UnityEngine;

public class HeartCollect : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.AddHealth(1);
            }

            Destroy(gameObject);
        }
    }
}