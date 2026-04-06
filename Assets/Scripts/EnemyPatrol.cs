using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    [Header("--- Movement Settings ---")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f; // Tốc độ khi đuổi theo player
    private bool movingRight = true;
    private bool isWaiting = false;

    [Header("--- Detection Settings ---")]
    public Transform groundCheck;
    public float groundDistance = 1f;
    public float wallDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("--- Player Chase Settings ---")]
    public float detectRange = 5f; // Khoảng cách quái nhìn thấy Player
    public LayerMask playerLayer; // Hãy tạo Layer "Player" và gán cho nhân vật
    private Transform playerTransform;
    private bool isChasing = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Tìm Player theo Tag (Đảm bảo nhân vật của bạn có Tag là "Player")
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    void Start()
    {
        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
            if (groundCheck == null && transform.childCount > 0)
                groundCheck = transform.GetChild(0);
        }
    }

    void Update()
    {
        if (groundCheck == null || isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckForPlayer();

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Move();
            ScanObstacles();
        }
    }

    void CheckForPlayer()
    {
        if (playerTransform == null) return;

        // Tính khoảng cách giữa quái và Player
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Kiểm tra xem Player có trong tầm mắt và cùng độ cao (trên cùng platform) không
        bool sameLevel = Mathf.Abs(transform.position.y - playerTransform.position.y) < 1.5f;

        if (distToPlayer <= detectRange && sameLevel)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    void ChasePlayer()
    {
        // Xác định hướng về phía Player
        float direction = playerTransform.position.x > transform.position.x ? 1 : -1;

        // Quay mặt về phía Player
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight))
        {
            Flip();
        }

        // Kiểm tra xem phía trước có vực hay tường không trước khi đuổi
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance, groundLayer);
        Vector2 wallDir = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(groundCheck.position, wallDir, wallDistance, groundLayer);

        // Nếu gặp vực hoặc tường thì dừng lại không đuổi nữa (tránh rơi xuống vực)
        if (groundHit.collider == null || wallHit.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        }
    }

    void Move()
    {
        float dir = movingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);
    }

    void ScanObstacles()
    {
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundDistance, groundLayer);
        Vector2 wallDir = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(groundCheck.position, wallDir, wallDistance, groundLayer);

        if (groundHit.collider == null || wallHit.collider != null)
        {
            StartCoroutine(WaitAndFlip());
        }
    }

    IEnumerator WaitAndFlip()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.5f);
        Flip();
        isWaiting = false;
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        // Vẽ tầm nhìn phát hiện Player (Vòng tròn xanh lá)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
    }
}