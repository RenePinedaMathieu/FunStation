using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class HighscoreEntry
{
    public string name;
    public int taps;
    public int misses;
    public long unixTime;
}

[Serializable]
public class HighscoreData
{
    public List<HighscoreEntry> entries = new List<HighscoreEntry>();
}

public static class HighscoreStorage
{
    const string KEY = "CIRCLE_TOP10_V2";

    public static List<HighscoreEntry> Load()
    {
        string json = PlayerPrefs.GetString(KEY, "");
        if (string.IsNullOrWhiteSpace(json))
            return new List<HighscoreEntry>();

        try
        {
            var data = JsonUtility.FromJson<HighscoreData>(json);
            return data?.entries ?? new List<HighscoreEntry>();
        }
        catch
        {
            return new List<HighscoreEntry>();
        }
    }

    static void Save(List<HighscoreEntry> entries)
    {
        var data = new HighscoreData { entries = entries };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static List<HighscoreEntry> RegisterScore(string name, int taps, int misses)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "Player";

        var list = Load();
        list.Add(new HighscoreEntry
        {
            name = name.Trim(),
            taps = taps,
            misses = misses,
            unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        // Sort: taps desc, misses asc, newest last as tie-break
        list = list
            .OrderByDescending(e => e.taps)
            .ThenBy(e => e.misses)
            .ThenBy(e => e.unixTime)
            .Take(10)
            .ToList();

        Save(list);
        return list;
    }
}
