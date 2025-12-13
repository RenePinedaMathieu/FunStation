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
        src.spatialBlend = 0f;     // 2D
        src.mute = false;
        if (src.volume <= 0f) src.volume = 1f;

        if (menuMusic != null)
            src.clip = menuMusic;
    }

    public void EnsurePlaying()
    {
        if (src == null) return;

        // Make sure global audio isn't paused/muted
        AudioListener.pause = false;
        if (AudioListener.volume <= 0f) AudioListener.volume = 1f;

        if (src.clip == null && menuMusic != null) src.clip = menuMusic;
        if (src.clip == null) return;

        src.mute = false;
        if (src.volume <= 0f) src.volume = 1f;

        // If it was paused, unpause; otherwise play
        if (!src.isPlaying)
        {
            // UnPause only works if it was paused before; Play is safe always
            src.UnPause();
            if (!src.isPlaying) src.Play();
        }
    }
}
