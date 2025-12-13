using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Services.Leaderboards.Models;
using System.Text.RegularExpressions;

public class HighscoreUI : MonoBehaviour
{
    [Header("Assign 10 TMP rows (Rank 1..10)")]
    public TMP_Text[] rows;

    // ✅ Local highscores (your PlayerPrefs system) - int-only fallback
    public void Render(List<int> scores)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (!rows[i]) continue;

            int rank = i + 1;
            if (i < scores.Count) rows[i].text = $"{rank}. {scores[i]} taps";
            else rows[i].text = $"{rank}. —";
        }
    }

    // ✅ Local highscores (name + taps)
    public void Render(List<HighscoreEntry> scores)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (!rows[i]) continue;

            int rank = i + 1;
            if (i < scores.Count)
                rows[i].text = $"{rank}. {scores[i].name} — {scores[i].taps}";
            else
                rows[i].text = $"{rank}. —";
        }
    }

    // ✅ Online UGS leaderboard
    public void RenderUGS(LeaderboardScoresPage page)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (!rows[i]) continue;

            int rank = i + 1;
            if (page != null && i < page.Results.Count)
            {
                var e = page.Results[i];

                // ✅ remove the "#1234" at the end (display only)
                string shownName = Regex.Replace(e.PlayerName ?? "", @"#\d{4}$", "");

                rows[i].text = $"{rank}. {shownName} — {(int)e.Score}";
            }
            else
            {
                rows[i].text = $"{rank}. —";
            }
        }
    }

}
