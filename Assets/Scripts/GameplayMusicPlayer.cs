using UnityEngine;
using UnityEngine.UI;

public class GameplayMusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicList;

    public Slider volumeSlider; // 👉 kéo slider vào đây

    int currentTrack = 0;

    void Start()
    {
        if (audioSource == null || musicList.Length == 0)
        {
            Debug.LogWarning("Music chưa được gán!");
            return;
        }

        // 👉 load volume đã lưu
        float savedVolume = PlayerPrefs.GetFloat("volume", 1f);

        // 👉 set volume cho nhạc
        audioSource.volume = savedVolume;

        // 👉 set lại slider (QUAN TRỌNG)
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;

            // 👉 đảm bảo slider chạy đúng
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        PlayRandomMusic();
    }

    void Update()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            PlayNextMusic();
        }
    }

    void PlayRandomMusic()
    {
        currentTrack = Random.Range(0, musicList.Length);

        audioSource.clip = musicList[currentTrack];
        audioSource.Play();

        Debug.Log("Playing: " + audioSource.clip.name);
    }

    void PlayNextMusic()
    {
        currentTrack++;

        if (currentTrack >= musicList.Length)
            currentTrack = 0;

        audioSource.clip = musicList[currentTrack];
        audioSource.Play();

        Debug.Log("Next: " + audioSource.clip.name);
    }

    public void NextMusic()
    {
        PlayNextMusic();
    }

    public void PrevMusic()
    {
        currentTrack--;

        if (currentTrack < 0)
            currentTrack = musicList.Length - 1;

        audioSource.clip = musicList[currentTrack];
        audioSource.Play();
    }

    // 🔥 FIX CHUẨN SLIDER
    public void SetVolume(float value)
    {
        if (audioSource == null) return;

        // 👉 clamp tránh tắt hoàn toàn
        float volume = Mathf.Clamp(value, 0.05f, 1f);

        audioSource.volume = volume;

        PlayerPrefs.SetFloat("volume", volume);
        PlayerPrefs.Save();
    }
}