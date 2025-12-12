using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class IntroScreenController : MonoBehaviour
{
    [Header("Flujo")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float failSafeSeconds = 15f;

    [Header("UI")]
    public GameObject startPanel;      // tu panel con logo + botón

    [Header("Video")]
    public VideoPlayer videoPlayer;    // referencia al VideoPlayer
    public string videoFileName = "sinmi_intro.mp4";

    private bool alreadyLoading = false;
    private bool started = false;

    // Este método lo llama el botón "Tap to Start"
    public void OnStartButton()
    {
        if (started) return;
        started = true;

        // Ocultamos el panel de UI
        if (startPanel != null)
            startPanel.SetActive(false);

        if (videoPlayer == null)
        {
            // Si no hay video, vamos directo al menú
            LoadNext();
            return;
        }

        // Configurar el video desde StreamingAssets
        string url = Application.streamingAssetsPath + "/" + videoFileName;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        // Suscribirnos a eventos
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;

        // Reproducir
        videoPlayer.Play();

        // Por si el video falla silenciosamente, forzamos salida
        StartCoroutine(FailSafe());
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        LoadNext();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogWarning("Error de video: " + message);
        LoadNext();
    }

    private IEnumerator FailSafe()
    {
        yield return new WaitForSeconds(failSafeSeconds);
        LoadNext();
    }

    private void LoadNext()
    {
        if (alreadyLoading) return;
        alreadyLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
