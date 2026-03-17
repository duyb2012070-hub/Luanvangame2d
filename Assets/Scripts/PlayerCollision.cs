using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    private Rigidbody2D rb;

    [Header("Fall Settings")]
    [SerializeField] private float fallLimit = -10f;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 6f;

    [Header("Sound Effects")]
    public AudioSource coinSound;
    public AudioSource trapSound;
    public AudioSource heartSound;

    private bool isDead = false;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
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

    // =========================
    // COIN + HEART
    // =========================
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            if (coinSound != null)
                coinSound.PlayOneShot(coinSound.clip);

            Destroy(collision.gameObject);

            if (gameManager != null)
                gameManager.AddScore(1);
        }

        if (collision.CompareTag("Heart"))
        {
            if (heartSound != null)
                heartSound.PlayOneShot(heartSound.clip);

            Destroy(collision.gameObject);

            if (gameManager != null)
                gameManager.AddHealth(1);
        }
    }

    // =========================
    // TRAP + ENEMY
    // =========================
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // TRAP
        if (collision.gameObject.CompareTag("Trap"))
        {
            DamagePlayer();
            return;
        }

        // ENEMY
        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // nếu player chạm từ trên xuống
                if (contact.normal.y < -0.5f)
                {
                    Bounce();
                    return;
                }
            }

            // nếu không phải đạp đầu → mất máu
            DamagePlayer();
        }
    }

    // =========================
    // BOUNCE
    // =========================
    void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    // =========================
    // DAMAGE
    // =========================
    void DamagePlayer()
    {
        if (isDead) return;

        isDead = true;

        if (trapSound != null)
            trapSound.PlayOneShot(trapSound.clip);

        if (gameManager != null)
            gameManager.TakeDamage();

        Invoke(nameof(ResetDeathState), 0.5f);
    }

    // =========================
    // FALL
    // =========================
    void PlayerFallDelay()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerFall();
        }

        Invoke(nameof(ResetDeathState), 0.5f);
    }

    void ResetDeathState()
    {
        isDead = false;
    }
}