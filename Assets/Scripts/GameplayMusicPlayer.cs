using UnityEngine;

public class GameplayMusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicList;

    void Start()
    {
        PlayRandomMusic();
    }

    void PlayRandomMusic()
    {
        int index = Random.Range(0, musicList.Length);
        audioSource.clip = musicList[index];
        audioSource.Play();
    }
}