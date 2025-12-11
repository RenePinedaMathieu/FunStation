using UnityEngine;
using UnityEngine.SceneManagement;

namespace SinmiStation.UI
{
    public class BackToMenuButton : MonoBehaviour
    {
            public string mainMenuSceneName = "MainMenu";

            public void OnBackButton()
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
    }
}
