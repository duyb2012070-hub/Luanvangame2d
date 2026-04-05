using UnityEngine;
using TMPro; // Đừng quên thư viện này nếu dùng TextMeshPro

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Kéo object TimerText vào đây
    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (isRunning)
        {
            // Tăng thời gian theo thời gian thực của game
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay(elapsedTime);
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        // Tính toán phút và giây
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Hiển thị định dạng 00:00
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Hàm để dừng thời gian khi thắng/thua
    public void StopTimer()
    {
        isRunning = false;
    }
}