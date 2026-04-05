using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool activated = false;
    private Animator anim; // Khai báo thêm Animator

    void Start()
    {
        // Lấy Component Animator khi bắt đầu
        anim = GetComponent<Animator>();

        // Giữ nguyên logic cũ của bạn
        transform.position += new Vector3(0f, -0.4f, 0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            activated = true;

            // KÍCH HOẠT ANIMATION Ở ĐÂY
            if (anim != null)
            {
                anim.SetTrigger("activate");
            }

            // Logic của GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.SetCheckpoint(transform.position);
            }

            Debug.Log("Checkpoint Activated with Animation!");
        }
    }
}