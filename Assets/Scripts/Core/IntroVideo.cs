using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideo : MonoBehaviour
{
    [Header("Nombre de la siguiente escena")]
    public string nextScene = "MainMenu";

    private VideoPlayer vp;

    void Start()
    {
        vp = GetComponent<VideoPlayer>();

        // Cuando el video termina, se llama a este evento
        vp.loopPointReached += OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        SceneManager.LoadScene(nextScene);
    }

    void Update()
    {
        // OPCIONAL: permitir saltar el video con click o toque
        if (Input.anyKeyDown || Input.touchCount > 0)
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
