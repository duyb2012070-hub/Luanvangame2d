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
        if (collision.CompareTag("Coin"))
        {
            if (coinSound != null)
                coinSound.PlayOneShot(coinSound.clip);

            Destroy(collision.gameObject);
            gameManager.AddScore(1);
        }

        if (collision.CompareTag("Trap") && !isDead)
        {
            if (trapSound != null)
                trapSound.PlayOneShot(trapSound.clip);

            gameManager.TakeDamage();
        }
    }

    void PlayerFallDelay()
    {
        GameManager.instance.PlayerFall();
    }
}