using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRouter : MonoBehaviour
{
    void Start()
    {
        int selectedGame = PlayerPrefs.GetInt("selectedGame", 0);

        // Nombres exactos de tus escenas:
        string target = (selectedGame == 0) ? "Game_Circle" : "Game_Stack";
        SceneManager.LoadScene(target);
    }
}
