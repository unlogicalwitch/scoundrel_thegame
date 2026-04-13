using TMPro;
using UnityEngine;

/// <summary>
/// Displays the remaining draw-pile count on a TextMeshPro label.
/// Subscribes to DeckManager.OnDeckCountChanged — updates whenever a card
/// is drawn, returned to the deck, or the deck is shuffled.
///
/// Wiring: GameStateMachine calls Initialise(deckManager) after building GameContext.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DrawDeckView : MonoBehaviour
{
    // ── Runtime ──────────────────────────────────────────────────────

    private TextMeshPro label;
    private DeckManager deckManager;

    // ── Setup ────────────────────────────────────────────────────────

    private void Awake()
    {
        label = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// Called by GameStateMachine after GameContext is built.
    /// </summary>
    public void Initialise(DeckManager deck)
    {
        deckManager = deck;
        deckManager.OnDeckCountChanged += HandleDeckCountChanged;

        // Sync immediately with the current count.
        HandleDeckCountChanged(deckManager.DrawPileCount);
    }

    private void OnDestroy()
    {
        if (deckManager != null)
            deckManager.OnDeckCountChanged -= HandleDeckCountChanged;
    }

    // ── Event handler ─────────────────────────────────────────────────

    private void HandleDeckCountChanged(int count)
    {
        if (label != null)
            label.text = count.ToString();
    }
}
