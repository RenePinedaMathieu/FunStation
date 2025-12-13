using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;

public class CircleGameManager : MonoBehaviour
{
    [Header("Gameplay")]
    public RectTransform spawnArea;
    public CritterTarget targetPrefab;       // ✅ was CircleTarget
    public float targetLifetime = 2f;
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

    // ✅ expose running to CritterTarget
    public bool IsRunning => running;

    CritterTarget current;

    // end/online guards
    bool ended = false;
    bool onlineRequestInFlight = false;

    void Start() => StartGame();

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

        if (instructions) instructions.SetActive(true);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        UpdateHUD();
        SpawnNewTarget();
    }

    public void EndGame()
    {
        if (ended) return;
        ended = true;

        running = false;

        if (current) Destroy(current.gameObject);

        if (finalScoreText) finalScoreText.text = $"Taps: {score}";
        if (finalMissText) finalMissText.text = $"Misses: {misses}";

        // local highscores
        if (highscoreUI)
        {
            string playerName = PlayerPrefs.GetString("playerName", "Player");
            var list = HighscoreStorage.RegisterScore(playerName, score, misses);
            highscoreUI.Render(list);
        }

        if (gameOverPanel) gameOverPanel.SetActive(true);

        _ = SubmitOnlineOnce();
    }

    async Task SubmitOnlineOnce()
    {
        if (onlineRequestInFlight) return;
        onlineRequestInFlight = true;

        try
        {
            if (UGSBoot.InitTask != null) await UGSBoot.InitTask;
            if (!UGSBoot.Ready) return;

            await LeaderboardsService.Instance.AddPlayerScoreAsync("Circles", score);
            var top10 = await LeaderboardsService.Instance.GetScoresAsync("Circles");
            if (highscoreUI) highscoreUI.RenderUGS(top10);
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

    void SpawnNewTarget()
    {
        if (!running) return;

        current = Instantiate(targetPrefab, spawnArea);
        current.Init(this, spawnArea, targetLifetime, spawnPadding);

        // random anchored position
        Rect r = spawnArea.rect;
        float xMin = r.xMin + spawnPadding;
        float xMax = r.xMax - spawnPadding;
        float yMin = r.yMin + spawnPadding;
        float yMax = r.yMax - spawnPadding;

        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);

        current.Rect.anchoredPosition = new Vector2(x, y);
    }

    // called by CritterTarget when tapped (shows death sprite first)
    public void OnTargetTapped(CritterTarget t)
    {
        if (!running) return;
        score++;
        UpdateHUD();

        // spawn next after death sprite shows
        float delay = Mathf.Max(0.05f, t != null ? t.deathShowTime : 0.2f);
        Invoke(nameof(SpawnNewTarget), delay);
    }

    // called by CritterTarget when lifetime ends
    public void OnTargetExpired(CritterTarget t)
    {
        if (!running) return;
        SpawnNewTarget();
    }

    // background tap catcher (miss taps)
    public void OnMissTap()
    {
        if (!running) return;
        misses++;
        UpdateHUD();
    }

    public void Retry() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void QuitGame() => SceneManager.LoadScene(quitSceneName);

    public void QuitRunNow()
    {
        if (!running) return;
        EndGame();
    }
}
