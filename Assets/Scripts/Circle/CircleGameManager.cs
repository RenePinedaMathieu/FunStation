using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CircleGameManager : MonoBehaviour
{
    [Header("Gameplay")]
    public RectTransform spawnArea;              // full-screen panel area (RectTransform)
    public CircleTarget circlePrefab;            // UI prefab
    public float circleLifetime = 2f;
    public float gameDuration = 60f;
    public float spawnPadding = 90f;            // keep circle away from edges

    [Header("UI (HUD)")]
    public TMP_Text scoreText;
    public TMP_Text missText;
    public TMP_Text timerText;

    [Header("UI (Instructions)")]
    public GameObject instructions;             // your instructions text object

    [Header("UI (Game Over)")]
    public GameObject gameOverPanel;            // panel with highscore + buttons
    public TMP_Text finalScoreText;
    public TMP_Text finalMissText;
    public HighscoreUI highscoreUI;

    [Header("Scene Navigation")]
    public string quitSceneName = "MainMenu";   // change to your menu scene name

    int score;
    int misses;
    float timeLeft;
    bool running;

    CircleTarget current;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame();
            return;
        }

        UpdateHUD();
    }

    public void StartGame()
    {
        running = true;
        score = 0;
        misses = 0;
        timeLeft = gameDuration;

        if (instructions) instructions.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        UpdateHUD();
        SpawnNewCircle();
    }

void EndGame()
{
    running = false;

    if (current) Destroy(current.gameObject);

    if (finalScoreText) finalScoreText.text = $"Taps: {score}";
    if (finalMissText) finalMissText.text = $"Misses: {misses}";

    // update + render highscores (top 10) WITH NAME
    if (highscoreUI)
    {
        string playerName = PlayerPrefs.GetString("playerName", "Player");
        var list = HighscoreStorage.RegisterScore(playerName, score, misses);
        highscoreUI.Render(list);
    }

    if (gameOverPanel) gameOverPanel.SetActive(true);
}


    void UpdateHUD()
    {
        if (scoreText) scoreText.text = $"Taps: {score}";
        if (missText) missText.text = $"Miss: {misses}";
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(timeLeft)}";
    }

    void SpawnNewCircle()
    {
        if (!running) return;

        if (current) Destroy(current.gameObject);

        current = Instantiate(circlePrefab, spawnArea);
        current.Init(this, circleLifetime);

        // Random anchored position inside spawnArea rect (with padding)
        Rect r = spawnArea.rect;
        float xMin = r.xMin + spawnPadding;
        float xMax = r.xMax - spawnPadding;
        float yMin = r.yMin + spawnPadding;
        float yMax = r.yMax - spawnPadding;

        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);

        current.Rect.anchoredPosition = new Vector2(x, y);
        current.Rect.localScale = Vector3.one;
    }

    // called by circle when tapped
    public void OnCircleTapped()
    {
        if (!running) return;
        score++;
        UpdateHUD();
        SpawnNewCircle();
    }

    // called by circle when it expires (shrinks to 0)
    public void OnCircleExpired()
    {
        if (!running) return;
        SpawnNewCircle();
    }

    // called by background tap catcher (miss taps)
    public void OnMissTap()
    {
        if (!running) return;
        misses++;
        UpdateHUD();
    }

    // UI buttons
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(quitSceneName);
    }

    // optional: quit early and show gameover panel
    public void QuitRunNow()
    {
        if (!running) return;
        EndGame();
    }
}
