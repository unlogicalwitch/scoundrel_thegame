using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the Scoundrel deck: filtering, shuffling, drawing, and the discard pile.
/// Pure C# — no MonoBehaviour. Owns no runtime card state.
///
/// Scoundrel deck = standard 52 cards
///   minus red Aces (Hearts Ace + Diamonds Ace)
///   minus Jokers (if any)
///   = 44 cards
/// </summary>
public class DeckManager
{
    // ── State ────────────────────────────────────────────────────────

    private readonly List<CardSO> drawPile    = new();
    private readonly List<CardSO> discardPile = new();

    // ── Events ───────────────────────────────────────────────────────

    /// Fired after every shuffle. Passes remaining draw pile count.
    public event Action<int> OnDeckShuffled;

    /// Fired each time a card is drawn. Passes the card + remaining count.
    public event Action<CardSO, int> OnCardDrawn;

    /// Fired when the draw pile is empty.
    public event Action OnDeckExhausted;

    // ── Public API ───────────────────────────────────────────────────

    public int DrawPileCount  => drawPile.Count;
    public int DiscardCount   => discardPile.Count;
    public bool IsDeckEmpty   => drawPile.Count == 0;

    /// <summary>
    /// Load all CardSO assets, strip non-Scoundrel cards, then shuffle.
    /// Call once at game start.
    /// </summary>
    /// <param name="allCards">All 52+ CardSO assets from the project.</param>
    public void Initialise(IEnumerable<CardSO> allCards)
    {
        drawPile.Clear();
        discardPile.Clear();

        foreach (var card in allCards)
        {
            if (card == null) continue;
            if (!card.IsRemovedFromScoundrelDeck)
                drawPile.Add(card);
        }

        Shuffle();
    }

    /// <summary>
    /// Fisher-Yates shuffle of the draw pile.
    /// </summary>
    public void Shuffle()
    {
        int n = drawPile.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }
        OnDeckShuffled?.Invoke(drawPile.Count);
    }

    /// <summary>
    /// Draw the top card from the draw pile.
    /// Returns null and fires OnDeckExhausted if the pile is empty.
    /// </summary>
    public CardSO Draw()
    {
        if (drawPile.Count == 0)
        {
            OnDeckExhausted?.Invoke();
            return null;
        }

        var card = drawPile[^1];
        drawPile.RemoveAt(drawPile.Count - 1);
        OnCardDrawn?.Invoke(card, drawPile.Count);
        return card;
    }

    /// <summary>
    /// Draw multiple cards at once. Returns fewer cards if the deck runs out.
    /// </summary>
    public List<CardSO> DrawMultiple(int count)
    {
        var drawn = new List<CardSO>(count);
        for (int i = 0; i < count; i++)
        {
            var card = Draw();
            if (card == null) break;
            drawn.Add(card);
        }
        return drawn;
    }

    /// <summary>
    /// Move a card to the discard pile. Call this after resolving a room card.
    /// </summary>
    public void Discard(CardSO card)
    {
        if (card == null) return;
        discardPile.Add(card);
    }

    /// <summary>
    /// Return room cards to the bottom of the draw pile (used when fleeing a room).
    /// Order is preserved — first card in the list ends up deepest.
    /// </summary>
    public void ReturnToDeck(IList<CardSO> cards)
    {
        for (int i = 0; i < cards.Count; i++)
            drawPile.Insert(i, cards[i]);
    }

    /// <summary>
    /// Read-only snapshot of the draw pile for debugging or UI.
    /// Do not mutate the returned list.
    /// </summary>
    public IReadOnlyList<CardSO> PeekDrawPile()  => drawPile.AsReadOnly();
    public IReadOnlyList<CardSO> PeekDiscard()   => discardPile.AsReadOnly();
}
