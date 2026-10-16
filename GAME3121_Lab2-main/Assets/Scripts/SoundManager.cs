using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public AudioSource sfxSource;

    void Awake()
    {
        Instance = this;
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip, volume);
    }
}
