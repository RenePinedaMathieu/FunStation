using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.IO;

public class IntroController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoFileName = "sinmi_intro.mp4";

    [Header("Flow")]
    public string nextSceneName = "MainMenu";
    public float failSafeSeconds = 12f;   // por si el video no arranca

    private bool alreadyLoading = false;
    private bool started = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;

        // Nunca nos quedamos pegados, aunque el video falle
        StartCoroutine(FailSafe());
    }

    // Llamado desde un botón "Tap to start" o similar
    public void StartIntro()
    {
        if (started || videoPlayer == null) return;
        started = true;

        string url = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    public void SkipIntro()
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (alreadyLoading) return;
        alreadyLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }

    System.Collections.IEnumerator FailSafe()
    {
        yield return new WaitForSeconds(failSafeSeconds);
        LoadNextScene();
    }
}
