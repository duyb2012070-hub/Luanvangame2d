using UnityEngine;
using UnityEngine.UI;

public class GameplayMusicPlayer : MonoBehaviour
{
    [Header("--- Cấu hình Âm thanh ---")]
    public AudioSource audioSource;
    public AudioClip[] musicList;
    public Slider volumeSlider;

    private int currentTrack = 0;

    void Start()
    {
        // 1. Kiểm tra xem đã từng có dữ liệu lưu chưa (HasKey)
        // Nếu chưa từng lưu (người mới), ép nó về 0.5f.
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.5f);
        }

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        // 2. Thiết lập Slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // Gán giá trị 0.5f (hoặc giá trị đã lưu) cho thanh trượt
            volumeSlider.value = savedVolume;

            // Xóa và gán lại sự kiện để đảm bảo không bị trùng lặp
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // 3. Áp dụng âm lượng ngay lập tức
        ApplyVolume(savedVolume);

        PlayMusic();
    }

    private void ApplyVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            // Tự động Mute nếu kéo về hết bên trái
            audioSource.mute = (volume <= 0.001f);
            audioSource.enabled = true;
        }
    }

    public void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        // Lưu lại để lần sau vào game vẫn giữ mức này
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save(); // Ép hệ thống ghi xuống ổ cứng
    }

    void PlayMusic()
    {
        if (musicList != null && musicList.Length > 0 && audioSource != null)
        {
            audioSource.clip = musicList[currentTrack];
            audioSource.Play();
        }
    }

    void Update()
    {
        // Kiểm tra chuyển bài khi hết nhạc
        if (audioSource != null && !audioSource.isPlaying && Time.timeScale > 0)
        {
            if (audioSource.clip != null) NextTrack();
        }
    }

    public void NextTrack()
    {
        if (musicList == null || musicList.Length == 0) return;
        currentTrack = (currentTrack + 1) % musicList.Length;
        PlayMusic();
    }
}