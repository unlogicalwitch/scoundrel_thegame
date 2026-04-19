using System;
using UnityEngine;

/// <summary>
/// Pure C# class that manages game settings with persistence via PlayerPrefs.
/// Fires events when settings change so views and logic can react.
/// </summary>
public class GameSettings
{
    // ── Constants ────────────────────────────────────────────────────
    
    private const string PREF_CONSECUTIVE_FLEE_ALLOWED = "ConsecutiveFleeAllowed";
    
    // ── Events ───────────────────────────────────────────────────────
    
    /// <summary>
    /// Fired when the consecutive flee setting changes.
    /// Passes the new value (true = allowed, false = restricted).
    /// </summary>
    public event Action<bool> OnConsecutiveFleeChanged;
    
    // ── Properties ───────────────────────────────────────────────────
    
    private bool consecutiveFleeAllowed;
    
    /// <summary>
    /// If true, player can flee multiple rooms in a row.
    /// If false, player cannot flee two rooms consecutively (original Scoundrel rule).
    /// </summary>
    public bool ConsecutiveFleeAllowed
    {
        get => consecutiveFleeAllowed;
        set
        {
            if (consecutiveFleeAllowed == value) return;
            
            consecutiveFleeAllowed = value;
            PlayerPrefs.SetInt(PREF_CONSECUTIVE_FLEE_ALLOWED, value ? 1 : 0);
            PlayerPrefs.Save();
            
            OnConsecutiveFleeChanged?.Invoke(value);
            Debug.Log($"[GameSettings] ConsecutiveFleeAllowed set to {value}");
        }
    }
    
    // ── Constructor ──────────────────────────────────────────────────
    
    /// <summary>
    /// Loads settings from PlayerPrefs. Defaults to false (restricted) if not set.
    /// </summary>
    public GameSettings()
    {
        // Default to false (original Scoundrel rule: no consecutive flee)
        consecutiveFleeAllowed = PlayerPrefs.GetInt(PREF_CONSECUTIVE_FLEE_ALLOWED, 0) == 1;
        Debug.Log($"[GameSettings] Loaded ConsecutiveFleeAllowed: {consecutiveFleeAllowed}");
    }
    
    // ── Public API ───────────────────────────────────────────────────
    
    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        ConsecutiveFleeAllowed = false;
        Debug.Log("[GameSettings] Reset to defaults");
    }
}
