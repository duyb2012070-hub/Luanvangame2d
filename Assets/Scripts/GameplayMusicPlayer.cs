using UnityEngine;
using UnityEngine.UI;

public class GameplayMusicPlayer : MonoBehaviour
{
    [Header("--- Cấu hình Âm thanh ---")]
    public AudioSource audioSource;
    public AudioClip[] musicList;
    public Slider volumeSlider; // Kéo cái Slider ở Pause Panel vào đây

    private int currentTrack = 0;

    void Start()
    {
        // 1. Lấy cài đặt On/Off từ Menu
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        audioSource.mute = !isMusicOn;

        // 2. Thiết lập âm lượng mặc định (0.5f)
        float savedVolume = 0.5f;
        audioSource.volume = savedVolume;

        // 3. ĐỒNG BỘ SLIDER (Để nó không bị sát trái)
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume; // Đẩy thanh gạt về giữa (0.5)

            // Gán sự kiện khi kéo Slider
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        PlayMusic();
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            NextTrack();
        }
    }

    public void OnVolumeChanged(float value)
    {
        if (audioSource != null)
        {
            audioSource.volume = value;
            // Nếu kéo về 0 thì tự động mute cho triệt để
            audioSource.mute = (value <= 0);
        }
    }

    void PlayMusic()
    {
        if (musicList.Length > 0)
        {
            audioSource.clip = musicList[currentTrack];
            audioSource.Play();
        }
    }

    void NextTrack()
    {
        currentTrack = (currentTrack + 1) % musicList.Length;
        PlayMusic();
    }
}