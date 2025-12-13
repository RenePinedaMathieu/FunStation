using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HighscoreUI : MonoBehaviour
{
    public TMP_Text[] rows; // size 10

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
}
