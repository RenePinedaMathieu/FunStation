using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;

public class CircleGameManager : MonoBehaviour
{
    [Header("Gameplay")]
    public RectTransform spawnArea;
    public CircleTarget circlePrefab;
    public float circleLifetime = 2f;
    public float gameDuration = 60f;
    public float spawnPadding = 90f;

    [Header("UI (HUD)")]
    public TMP_Text scoreText;
    public TMP_Text missText;
    public TMP_Text timerText;

    [Header("UI (Instructions)")]
    public GameObject instructions;

    [Header("UI (Game Over)")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalMissText;
    public HighscoreUI highscoreUI;

    [Header("Scene Navigation")]
    public string quitSceneName = "MainMenu";

    int score;
    int misses;
    float timeLeft;
    bool running;

    CircleTarget current;

    // ✅ guards
    bool ended = false;
    bool onlineRequestInFlight = false;

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
        ended = false;
        onlineRequestInFlight = false;

        running = true;
        score = 0;
        misses = 0;
        timeLeft = gameDuration;

        if (instructions) instructions.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        UpdateHUD();
        SpawnNewCircle();
    }

    // ✅ public entrypoint (can be called from timer or quit button)
    public void EndGame()
    {
        if (ended) return;   // prevents double calls
        ended = true;

        running = false;

        if (current) Destroy(current.gameObject);

        // Final UI
        if (finalScoreText) finalScoreText.text = $"Taps: {score}";
        if (finalMissText) finalMissText.text = $"Misses: {misses}";

        // Local highscores immediately (fast + works offline)
        if (highscoreUI)
        {
            string playerName = PlayerPrefs.GetString("playerName", "Player");
            var list = HighscoreStorage.RegisterScore(playerName, score, misses);
            highscoreUI.Render(list);
        }

        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Online submit/fetch in background (won't spam)
        _ = SubmitOnlineOnce();
    }

    async Task SubmitOnlineOnce()
    {
        if (onlineRequestInFlight) return;
        onlineRequestInFlight = true;

        try
        {
            if (UGSBoot.InitTask != null)
                await UGSBoot.InitTask;

            if (!UGSBoot.Ready)
            {
                Debug.LogWarning("UGS not ready; skipping online leaderboard.");
                return;
            }

            // Submit + fetch top 10
            await LeaderboardsService.Instance.AddPlayerScoreAsync("Circles", score);
            var top10 = await LeaderboardsService.Instance.GetScoresAsync("Circles");

            if (highscoreUI)
                highscoreUI.RenderUGS(top10);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Online leaderboard failed: " + e.Message);
        }
        finally
        {
            onlineRequestInFlight = false;
        }
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

    public void OnCircleTapped()
    {
        if (!running) return;
        score++;
        UpdateHUD();
        SpawnNewCircle();
    }

    public void OnCircleExpired()
    {
        if (!running) return;
        SpawnNewCircle();
    }

    public void OnMissTap()
    {
        if (!running) return;
        misses++;
        UpdateHUD();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(quitSceneName);
    }

    public void QuitRunNow()
    {
        if (!running) return;
        EndGame();
    }
}
