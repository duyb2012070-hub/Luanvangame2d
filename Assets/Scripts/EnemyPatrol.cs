using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("--- Movement Settings ---")]
    public float speed = 2f;
    private bool movingRight = true;

    [Header("--- Ground Check ---")]
    // Vẫn để public để bạn có thể kéo thả trong Prefab nếu muốn
    public Transform groundCheck;
    public float groundDistance = 1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // TỰ ĐỘNG TÌM groundCheck NẾU BỊ TRỐNG (Cực kỳ quan trọng khi Load Game)
        if (groundCheck == null)
        {
            // Tìm đối tượng con có tên là "GroundCheck"
            groundCheck = transform.Find("GroundCheck");

            // Nếu vẫn không tìm thấy bằng tên, lấy đối tượng con đầu tiên
            if (groundCheck == null && transform.childCount > 0)
            {
                groundCheck = transform.GetChild(0);
            }
        }

        // Kiểm tra lại lần cuối để tránh lỗi Log liên tục
        if (groundCheck == null)
        {
            Debug.LogError($"⚠️ [{gameObject.name}] Chưa gán GroundCheck! Hãy tạo một Object con tên là 'GroundCheck'.");
        }
    }

    void Update()
    {
        // Nếu không có groundCheck thì không chạy logic để tránh báo lỗi Console
        if (groundCheck == null) return;

        Move();
        CheckGround();
    }

    void Move()
    {
        // Sử dụng velocity để di chuyển 2D
        float dir = movingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    void CheckGround()
    {
        // Bắn tia Raycast xuống dưới để kiểm tra mặt đất
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundDistance,
            groundLayer
        );

        // Nếu tia Raycast không chạm vào layer Ground -> Đến vực thẳm -> Quay đầu
        if (hit.collider == null)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;

        // Xoay hướng quái vật bằng cách đổi Scale X
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // Lưu ý: Nếu bạn xoay quái, hãy đảm bảo GroundCheck là con của Quái 
        // để nó cũng xoay theo sang phía trước đầu quái.
    }

    // Vẽ đường tia Raycast trong Scene để bạn dễ căn chỉnh groundDistance
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundDistance
        );
    }
}