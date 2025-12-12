using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GamePlaceholder");  // nombre de tu escena de juego
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleMute()
    {
        AudioListener.volume = (AudioListener.volume > 0f) ? 0f : volumeSlider.value;
    }

    public void SetVolume(float v)
    {
        AudioListener.volume = v;
    }
}
