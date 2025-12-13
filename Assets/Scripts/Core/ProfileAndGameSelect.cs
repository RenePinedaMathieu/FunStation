using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class ProfileAndGameSelect : MonoBehaviour
{
    [Header("Panels")]
    public GameObject namePanel;
    public GameObject gameSelectPanel;

    [Header("Name UI")]
    public TMP_InputField nameInput;

    [Header("Active Name Badge")]
    public GameObject nameBadge;
    public TMP_Text activeNameText;

    [Header("Tiles (Buttons)")]
    public Button tileTap;
    public Button tileStack;

    [Header("Yellow border (Outline)")]
    public Outline tapOutline;
    public Outline stackOutline;
    public Color borderColor = new Color(1f, 0.85f, 0.2f, 1f);
    public Vector2 borderSize = new Vector2(6f, 6f);

    int selectedGame = 0;

    void Start()
    {
        selectedGame = PlayerPrefs.GetInt("selectedGame", 0);

        SetupOutline(ref tapOutline, tileTap);
        SetupOutline(ref stackOutline, tileStack);
        ApplyBorder();

        if (tileTap) tileTap.onClick.AddListener(() => SelectGame(0));
        if (tileStack) tileStack.onClick.AddListener(() => SelectGame(1));

        var savedName = PlayerPrefs.GetString("playerName", "");
        bool hasName = !string.IsNullOrWhiteSpace(savedName);

        if (namePanel) namePanel.SetActive(!hasName);
        if (gameSelectPanel) gameSelectPanel.SetActive(hasName);

        RefreshNameBadge();
    }

    void SetupOutline(ref Outline outline, Button btn)
    {
        if (btn == null) return;

        var img = btn.GetComponent<Image>();
        if (img == null) img = btn.GetComponentInChildren<Image>();

        if (outline == null && img != null) outline = img.GetComponent<Outline>();
        if (outline == null && img != null) outline = img.gameObject.AddComponent<Outline>();

        if (outline != null)
        {
            outline.effectColor = borderColor;
            outline.effectDistance = borderSize;
            outline.useGraphicAlpha = false;
            outline.enabled = false;
        }
    }

    void RefreshNameBadge()
    {
        var n = PlayerPrefs.GetString("playerName", "").Trim();

        if (nameBadge != null)
            nameBadge.SetActive(!string.IsNullOrWhiteSpace(n));

        if (activeNameText != null)
            activeNameText.text = string.IsNullOrWhiteSpace(n) ? "" : $" {n}";
    }

    public void OnConfirmName()
    {
        if (nameInput == null) return;

        var n = nameInput.text.Trim();
        if (n.Length < 2) return;

        PlayerPrefs.SetString("playerName", n);
        PlayerPrefs.Save();

        if (namePanel) namePanel.SetActive(false);
        if (gameSelectPanel) gameSelectPanel.SetActive(true);

        RefreshNameBadge();

        // para WebGL (audio se desbloquea con un click)
        if (MusicManager.I != null) MusicManager.I.EnsurePlaying();

        // ✅ NEW: update UGS display name (non-blocking, doesn’t change method signature)
        _ = PushNameToUGSAsync(n);
    }

    public void ChangeUser()
    {
        PlayerPrefs.DeleteKey("playerName");
        PlayerPrefs.Save();

        if (nameInput) nameInput.text = "";

        if (namePanel) namePanel.SetActive(true);
        if (gameSelectPanel) gameSelectPanel.SetActive(false);

        RefreshNameBadge();
    }

    void SelectGame(int id)
    {
        selectedGame = id;
        PlayerPrefs.SetInt("selectedGame", id);
        PlayerPrefs.Save();

        ApplyBorder();
        RefreshNameBadge();

        if (MusicManager.I != null) MusicManager.I.EnsurePlaying();
    }

    void ApplyBorder()
    {
        if (tapOutline) tapOutline.enabled = (selectedGame == 0);
        if (stackOutline) stackOutline.enabled = (selectedGame == 1);
    }

    static async Task PushNameToUGSAsync(string rawName)
    {
        try
        {
            if (UGSBoot.InitTask != null) await UGSBoot.InitTask;
            if (!UGSBoot.Ready) return;

            string clean = SanitizeUGSName(rawName);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(clean);
        }
        catch (Exception e)
        {
            Debug.LogWarning("UGS name update failed: " + e.Message);
        }
    }

    static string SanitizeUGSName(string s)
    {
        s = (s ?? "Player").Trim();
        s = Regex.Replace(s, @"\s+", "_");
        if (s.Length > 50) s = s.Substring(0, 50);
        if (string.IsNullOrEmpty(s)) s = "Player";
        return s;
    }
}
