using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Drives all UI Toolkit HUD elements from model events.
/// Subscribes to PlayerState events and updates the UIDocument accordingly.
///
/// Wiring:
///   GameStateMachine calls Initialise(playerState) after building GameContext.
///   This view never reads GameContext directly — it only holds a PlayerState ref.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class HUDView : MonoBehaviour
{
    // ── Cached element references ─────────────────────────────────────

    private Label healthLabel;

    // ── Model reference ───────────────────────────────────────────────

    private PlayerState playerState;

    // ── Setup ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameStateMachine after GameContext is built.
    /// </summary>
    public void Initialise(PlayerState player)
    {
        playerState = player;

        // Query elements from the UIDocument on this same GameObject.
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthLabel = root.Q<Label>("HealthLabel");

        if (healthLabel == null)
            Debug.LogWarning("[HUDView] Could not find 'HealthLabel' in UIDocument.");

        // Subscribe to model events.
        playerState.OnHealthChanged += HandleHealthChanged;

        // Sync immediately with current state.
        HandleHealthChanged(playerState.CurrentHealth, playerState.MaxHealth);
    }

    private void OnDestroy()
    {
        if (playerState != null)
            playerState.OnHealthChanged -= HandleHealthChanged;
    }

    // ── Event handlers ────────────────────────────────────────────────

    private void HandleHealthChanged(int current, int max)
    {
        if (healthLabel != null)
            healthLabel.text = current.ToString();
    }
}
