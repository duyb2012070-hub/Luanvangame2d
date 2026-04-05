using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerCollision playerCollision; // Thêm biến này để gọi âm thanh

    private bool isGrounded;
    private bool canDoubleJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        // Lấy script Collision nằm cùng trên nhân vật
        playerCollision = GetComponent<PlayerCollision>();
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        UpdateAnimation();
    }

    void CheckGround()
    {
        bool groundHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool enemyHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, enemyLayer);

        isGrounded = groundHit || enemyHit;

        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            canDoubleJump = true;
            anim.SetBool("isJumping", false);
        }
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        anim.SetBool("isRunning", moveInput != 0 && isGrounded);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
                // Gọi âm thanh nhảy từ script Collision
                if (playerCollision != null) playerCollision.PlayJumpSfx();
            }
            else if (canDoubleJump)
            {
                Jump();
                // Gọi âm thanh nhảy đôi từ script Collision
                if (playerCollision != null) playerCollision.PlayJumpSfx();
                canDoubleJump = false;
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetBool("isJumping", true);
    }

    void UpdateAnimation()
    {
        if (!isGrounded)
        {
            anim.SetBool("isJumping", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}