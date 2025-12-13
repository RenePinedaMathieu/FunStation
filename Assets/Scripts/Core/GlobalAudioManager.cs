using System.Collections;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    public float fadeSeconds = 0.25f;

    AudioSource musicSource;
    AudioClip silenceClip;
    bool unlocked;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // 2D music
        musicSource.volume = 0f;

        // 1 second of silence (zeros)
        silenceClip = AudioClip.Create("silence", 44100, 1, 44100, false);
    }

    // CALL THIS ONLY FROM A USER TAP (Play button)
    public void UnlockFromUserGesture()
    {
        if (unlocked) return;
        unlocked = true;

        AudioListener.pause = false;
        AudioListener.volume = 1f;

        // Play a silent loop to unlock iOS WebGL audio
        musicSource.clip = silenceClip;
        musicSource.volume = 0.0001f;
        musicSource.loop = true;
        musicSource.Play();
    }


    public void PlayMenuMusic()
    {
        if (menuMusic == null) return;
        StartCoroutine(SwitchMusic(menuMusic, musicVolume));
    }

    public void PlayGameMusic()
    {
        if (gameMusic == null) return;
        StartCoroutine(SwitchMusic(gameMusic, musicVolume));
    }

    IEnumerator SwitchMusic(AudioClip clip, float targetVol)
    {
        // fade out
        yield return FadeTo(0f);

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        // fade in
        yield return FadeTo(targetVol);
    }

    IEnumerator FadeTo(float v)
    {
        float start = musicSource.volume;
        float t = 0f;

        float dur = Mathf.Max(0.01f, fadeSeconds);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(start, v, t / dur);
            yield return null;
        }
        musicSource.volume = v;
    }
}
