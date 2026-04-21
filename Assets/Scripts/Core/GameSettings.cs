using System;
using UnityEngine;

/// <summary>
/// Pure C# class that manages game settings with persistence via PlayerPrefs.
/// </summary>
public class GameSettings
{
    // ── Constants ────────────────────────────────────────────────────
    
    private const string PREF_CONSECUTIVE_FLEE_ALLOWED = "ConsecutiveFleeAllowed";
    private const string PREF_CONSUME_MULTIPLE_POTION = "ConsumeMultiplePotion";
    
    // ── Properties ───────────────────────────────────────────────────
    
    /// <summary>
    /// If true, player can flee multiple rooms in a row.
    /// If false, player cannot flee two rooms consecutively (original Scoundrel rule).
    /// </summary>
    public bool ConsecutiveFleeAllowed
    {
        get => PlayerPrefs.GetInt(PREF_CONSECUTIVE_FLEE_ALLOWED, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(PREF_CONSECUTIVE_FLEE_ALLOWED, value ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[GameSettings] ConsecutiveFleeAllowed set to {value}");
        }
    }
    
    /// <summary>
    /// If true, player can consume multiple potions in a room without losing effect.
    /// If false, only the first potion in a room has effect (original Scoundrel rule).
    /// </summary>
    public bool ConsumeMultiplePotionAllowed
    {
        get => PlayerPrefs.GetInt(PREF_CONSUME_MULTIPLE_POTION, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(PREF_CONSUME_MULTIPLE_POTION, value ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[GameSettings] ConsumeMultiplePotionAllowed set to {value}");
        }
    }
    
    // ── Constructor ──────────────────────────────────────────────────
    
    public GameSettings()
    {
        Debug.Log($"[GameSettings] Loaded ConsecutiveFleeAllowed: {ConsecutiveFleeAllowed}");
        Debug.Log($"[GameSettings] Loaded ConsumeMultiplePotionAllowed: {ConsumeMultiplePotionAllowed}");
    }
    
    // ── Public API ───────────────────────────────────────────────────
    
    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        ConsecutiveFleeAllowed = false;
        ConsumeMultiplePotionAllowed = false;
        Debug.Log("[GameSettings] Reset to defaults");
    }
}
