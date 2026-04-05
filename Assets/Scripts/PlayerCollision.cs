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
    public AudioSource jumpSound; // <--- THÊM MỚI: Kéo AudioSource nhảy vào đây

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
            PlaySfx(trapSound); // Sử dụng hàm dùng chung để kiểm tra SFX On/Off

            Invoke(nameof(PlayerFallDelay), 0.2f);
        }
    }

    // ==========================================
    // --- HÀM PHÁT ÂM THANH DÙNG CHUNG ---
    // ==========================================
    void PlaySfx(AudioSource source)
    {
        // Kiểm tra xem người dùng có bật Sound trong Menu không
        if (PlayerPrefs.GetInt("SfxOn", 1) == 1)
        {
            if (source != null && source.clip != null)
            {
                source.PlayOneShot(source.clip);
            }
        }
    }

    // Hàm này để PlayerController gọi sang khi nhảy
    public void PlayJumpSfx()
    {
        PlaySfx(jumpSound);
    }

    // =========================
    // COIN + HEART
    // =========================
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            PlaySfx(coinSound); // Kiểm tra SFX On/Off
            Destroy(collision.gameObject);

            if (gameManager != null)
                gameManager.AddScore(1);
        }

        if (collision.CompareTag("Heart"))
        {
            PlaySfx(heartSound); // Kiểm tra SFX On/Off
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

        if (collision.gameObject.CompareTag("Trap"))
        {
            DamagePlayer();
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    Bounce();
                    return;
                }
            }
            DamagePlayer();
        }
    }

    void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    void DamagePlayer()
    {
        if (isDead) return;
        isDead = true;

        PlaySfx(trapSound); // Kiểm tra SFX On/Off

        if (gameManager != null)
            gameManager.TakeDamage();

        Invoke(nameof(ResetDeathState), 0.5f);
    }

    void PlayerFallDelay()
    {
        if (GameManager.instance != null)
            GameManager.instance.PlayerFall();

        Invoke(nameof(ResetDeathState), 0.5f);
    }

    void ResetDeathState()
    {
        isDead = false;
    }
}