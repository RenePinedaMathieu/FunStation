using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;

[DefaultExecutionOrder(-10000)]
public class UGSBoot : MonoBehaviour
{
    public static bool Ready { get; private set; }
    public static Task InitTask { get; private set; } = Task.CompletedTask;

    static bool started;

    void Awake()
    {
        // singleton
        var existing = FindObjectsByType<UGSBoot>(FindObjectsSortMode.None);
        if (existing.Length > 1) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        if (!started)
        {
            started = true;
            InitTask = InitAsync();   // start init as early as possible
        }
    }

    static async Task InitAsync()
    {
        try
        {
            // Force production to match your dashboard dropdown
            var options = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(options);

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("CloudProjectId (from build): " + Application.cloudProjectId);
            Debug.Log("SignedIn: " + AuthenticationService.Instance.IsSignedIn);

            // Update player name (don't fail init if it errors)
            try
            {
                string playerName = SanitizeName(PlayerPrefs.GetString("playerName", "Player"));
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            }
            catch (Exception e)
            {
                Debug.LogWarning("UpdatePlayerName failed: " + e.Message);
            }

            Ready = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("UGS init failed: " + e.Message);
            Ready = false;
        }
    }

    static string SanitizeName(string s)
    {
        s = (s ?? "Player").Trim();
        s = Regex.Replace(s, @"\s+", "_"); // no spaces
        if (s.Length > 50) s = s.Substring(0, 50);
        if (string.IsNullOrEmpty(s)) s = "Player";
        return s;
    }
}
