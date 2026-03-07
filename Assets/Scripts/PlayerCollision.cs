using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private float fallLimit = -10f;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        // Nếu player rơi khỏi map
        if (transform.position.y < fallLimit)
        {
            gameManager.GameOver();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ăn coin
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            gameManager.AddScore(1);
        }
    }
}