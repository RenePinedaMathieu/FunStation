using UnityEngine;

public enum GameId { TapCircles, StackBoxes }

public class AppState : MonoBehaviour
{
    public static AppState I;

    public string playerName;
    public GameId selectedGame = GameId.TapCircles;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString("playerName", "");
    }

    public void SaveName(string name)
    {
        playerName = name;
        PlayerPrefs.SetString("playerName", name);
        PlayerPrefs.Save();
    }
}
