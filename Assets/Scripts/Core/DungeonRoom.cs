using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the 4 cards currently laid out in the dungeon room.
/// Tracks which cards have been resolved and whether the player has
/// fled this room before (you can only flee each room once).
///
/// Room-clear rule: after 3 of the 4 cards are resolved the 1 surviving
/// card stays in place and 3 fresh cards are dealt into the empty slots.
/// </summary>
public class DungeonRoom
{
    // ── State ────────────────────────────────────────────────────────

    private readonly List<CardSO> cards = new(4);
    private readonly List<CardSO> resolvedCards = new(4);

    // ── Events ───────────────────────────────────────────────────────

    public event Action<CardSO> OnCardResolved;
    /// <summary>Fired on the very first deal (full 4-card layout).</summary>
    public event Action<List<CardSO>> OnRoomDealt;
    /// <summary>
    /// Fired when 3 cards have been resolved and 3 new cards are added.
    /// Carries only the 3 NEW cards; the surviving card stays in its slot.
    /// </summary>
    public event Action<List<CardSO>> OnRoomRefilled;
    /// <summary>
    /// Fired when 3 cards have been resolved (1 card remains).
    /// DrawingState listens to this to draw 3 new cards and call RefillRoom().
    /// </summary>
    public event Action OnRoomCleared;
    public event Action OnRoomReady;
    public event Action OnRoomFled;

    // ── Public API ───────────────────────────────────────────────────

    public IReadOnlyList<CardSO> Cards => cards.AsReadOnly();
    public bool FledLastRoom { get; private set; }
    public bool PotionUsedThisRoom { get; private set; }
    public bool IsCleared => cards.Count == 0;
    public int RemainingCards => cards.Count;

    /// <summary>
    /// Set up a brand-new room with up to 4 cards drawn from the deck.
    /// Used at game start and after a flee.
    /// </summary>
    public void Deal(List<CardSO> newCards)
    {
        cards.Clear();
        resolvedCards.Clear();
        
        PotionUsedThisRoom = false;

        foreach (var card in newCards)
            if (card != null) cards.Add(card);

        OnRoomDealt?.Invoke(new List<CardSO>(cards));
    }

    /// <summary>
    /// Add 3 new cards to the room alongside the 1 surviving card.
    /// The surviving card keeps its existing slot; new cards fill the empty slots.
    /// Fires OnRoomRefilled with only the new cards.
    /// </summary>
    public void RefillRoom(List<CardSO> newCards)
    {
        resolvedCards.Clear();
        
        FledLastRoom = false;
        PotionUsedThisRoom = false;

        var added = new List<CardSO>(newCards.Count);
        foreach (var card in newCards)
        {
            if (card == null) continue;
            cards.Add(card);
            added.Add(card);
        }

        OnRoomRefilled?.Invoke(added);
    }

    /// <summary>
    /// Remove a card from the room after the player acts on it.
    /// Fires OnRoomCleared when exactly 1 card remains (3 resolved).
    /// </summary>
    public void Resolve(CardSO card)
    {
        if (!cards.Contains(card)) return;
        cards.Remove(card);
        resolvedCards.Add(card);
        OnCardResolved?.Invoke(card);
        
        if (card.Category == CardCategory.Potion)
            PotionUsedThisRoom = true;
        
        if (cards.Count == 1)
        {
            Debug.Log("[DungeonRoom] 3 cards resolved — room cleared, refilling...");
            OnRoomCleared?.Invoke();
        }
    }

    /// <summary>
    /// Called by RoomView once all deal/refill animations have finished.
    /// Fires OnRoomReady so DrawingState can transition to PlayerChoiceState.
    /// </summary>
    public void NotifyRoomReady()
    {
        OnRoomReady?.Invoke();
    }

    public IReadOnlyList<CardSO> Flee()
    {
        FledLastRoom = true;
        OnRoomFled?.Invoke();
        Debug.Log("[DungeonRoom] Flee");
        return cards.AsReadOnly();
    }
}
