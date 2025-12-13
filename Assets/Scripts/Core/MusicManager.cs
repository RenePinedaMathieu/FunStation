using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager I { get; private set; }

    public AudioClip menuMusic;

    AudioSource src;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;

        if (menuMusic != null)
            src.clip = menuMusic;
    }

    public void EnsurePlaying()
    {
        if (src == null) return;
        if (src.clip == null && menuMusic != null) src.clip = menuMusic;
        if (src.clip != null && !src.isPlaying) src.Play();
    }
}
