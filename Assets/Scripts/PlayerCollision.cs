using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private float fallLimit = -10f;

    [Header("Sound Effects")]
    public AudioSource coinSound;
    public AudioSource trapSound;

    private bool isDead = false;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void Update()
    {
        // kiểm tra rơi khỏi map
        if (!isDead && transform.position.y < fallLimit)
        {
            isDead = true;

            if (trapSound != null)
                trapSound.PlayOneShot(trapSound.clip);

            Invoke(nameof(PlayerFallDelay), 0.2f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // ăn coin
        if (collision.CompareTag("Coin"))
        {
            if (coinSound != null)
                coinSound.PlayOneShot(coinSound.clip);

            Destroy(collision.gameObject);

            if (gameManager != null)
                gameManager.AddScore(1);
        }

        // dính trap
        if (collision.CompareTag("Trap") && !isDead)
        {
            if (trapSound != null)
                trapSound.PlayOneShot(trapSound.clip);

            isDead = true;

            if (gameManager != null)
                gameManager.TakeDamage();

            // reset trạng thái sau khi respawn
            Invoke(nameof(ResetDeathState), 0.5f);
        }
    }

    void PlayerFallDelay()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerFall();
        }

        // reset trạng thái để lần sau còn rơi tiếp được
        Invoke(nameof(ResetDeathState), 0.5f);
    }

    void ResetDeathState()
    {
        isDead = false;
    }
}