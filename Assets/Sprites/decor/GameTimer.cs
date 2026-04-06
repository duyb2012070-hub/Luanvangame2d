using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Kéo object TimerText từ Canvas vào đây

    void Update()
    {
        // 1. Kiểm tra xem GameManager đã sẵn sàng chưa
        if (GameManager.instance != null)
        {
            // 2. Lấy trực tiếp thời gian thực từ GameManager (đã bao gồm thời gian nạp từ DB)
            float timeToDisplay = GameManager.instance.playTimer;

            // 3. Cập nhật hiển thị UI
            UpdateTimerDisplay(timeToDisplay);
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timerText == null) return;

        // Tính toán phút và giây từ con số tổng
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Hiển thị định dạng 00:00 (Ví dụ: 01:25)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Ghi chú: Bạn không cần hàm StopTimer ở đây nữa, 
    // vì khi GameManager.instance.isGameOver = true, 
    // biến playTimer bên GameManager sẽ ngừng tăng, UI sẽ tự động dừng theo.
}