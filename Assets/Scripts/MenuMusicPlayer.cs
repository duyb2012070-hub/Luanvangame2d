using UnityEngine;

public class MenuMusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicList;

    private int currentTrack = 0;

    void Start()
    {
        PlayMusic();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    void PlayMusic()
    {
        audioSource.clip = musicList[currentTrack];
        audioSource.Play();
    }

    void NextTrack()
    {
        currentTrack++;

        if (currentTrack >= musicList.Length)
        {
            currentTrack = 0;
        }

        PlayMusic();
    }
}