using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool activated = false;

    void Start()
    {
        // Hạ flag xuống một chút để nhìn tự nhiên hơn
        transform.position += new Vector3(0f, -0.4f, 0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            activated = true;

            GameManager.instance.SetCheckpoint(transform.position);

            Debug.Log("Checkpoint Activated!");
        }
    }
}