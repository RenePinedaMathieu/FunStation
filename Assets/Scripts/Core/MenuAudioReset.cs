using UnityEngine;

public class MenuAudioReset : MonoBehaviour
{
    void Awake()
    {
        AudioListener.pause = false;
        AudioListener.volume = 1f;
        Time.timeScale = 1f; // in case intro left it at 0
    }
}
