using UnityEngine;

namespace SinmiStation.Core
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;

        [Header("Audio Source")]
        public AudioSource audioSource;

        [Header("Music Clips")]
        public AudioClip mainTheme;   // música general de fondo

        private const string VolumeKey = "SS_MusicVolume";

        private void Awake()
        {
            // Patrón singleton básico
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // sobrevive entre escenas
        }

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // Volumen guardado (por si más adelante agregas slider)
            float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.5f);
            audioSource.volume = savedVolume;

            PlayMainTheme();
        }

        public void PlayMainTheme()
        {
            if (mainTheme == null || audioSource == null)
                return;

            if (audioSource.clip == mainTheme && audioSource.isPlaying)
                return;

            audioSource.loop = true;
            audioSource.clip = mainTheme;
            audioSource.Play();
        }

        public void SetVolume(float volume)
        {
            if (audioSource == null) return;

            volume = Mathf.Clamp01(volume);
            audioSource.volume = volume;
            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.Save();
        }

        public void ToggleMute()
        {
            if (audioSource == null) return;
            audioSource.mute = !audioSource.mute;
        }
    }
}
