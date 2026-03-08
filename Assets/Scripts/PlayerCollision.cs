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
        // Rơi khỏi map -> chết luôn
        if (transform.position.y < fallLimit)
        {
            GameManager.instance.PlayerFall();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Coin
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            gameManager.AddScore(1);
        }

        // Trap -> mất 1 tim
        if (collision.CompareTag("Trap"))
        {
            gameManager.TakeDamage();
        }
    }
}