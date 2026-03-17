using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    private Rigidbody2D rb;

    public float hoverHeight = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            Vector3 pos = transform.position;

            pos.y = col.collider.bounds.max.y + hoverHeight;

            transform.position = pos;

            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }
}