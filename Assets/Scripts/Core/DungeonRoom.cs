using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
    /// Represents the 4 cards currently laid out in the dungeon room.
    /// Tracks which cards have been resolved and whether the player has
    /// fled this room before (you can only flee each room once).
    /// </summary>
    public class DungeonRoom
    {
        // ── State ────────────────────────────────────────────────────────
 
        private readonly List<CardSO> cards        = new(4);
        private readonly List<CardSO> resolvedCards = new(4);
 
        // ── Events ───────────────────────────────────────────────────────
 
        public event Action<CardSO>        OnCardResolved;
        public event Action<IList<CardSO>> OnRoomDealt;
        public event Action                OnRoomCleared;
 
        // ── Public API ───────────────────────────────────────────────────
 
        public IReadOnlyList<CardSO> Cards         => cards.AsReadOnly();
        public bool                  HasFled        { get; private set; }
        public bool                  IsCleared      => cards.Count == 0;
        public int                   RemainingCount => cards.Count;
 
        /// <summary>
        /// Set up a new room with up to 4 cards drawn from the deck.
        /// </summary>
        public void Deal(IList<CardSO> newCards)
        {
            cards.Clear();
            resolvedCards.Clear();
            HasFled = false;
 
            foreach (var card in newCards)
                if (card != null) cards.Add(card);
 
            OnRoomDealt?.Invoke(cards.AsReadOnly());
        }
 
        /// <summary>
        /// Remove a card from the room after the player acts on it.
        /// </summary>
        public void Resolve(CardSO card)
        {
            if (!cards.Contains(card)) return;
            cards.Remove(card);
            resolvedCards.Add(card);
            OnCardResolved?.Invoke(card);
 
            if (cards.Count == 0)
                OnRoomCleared?.Invoke();
        }
 
        /// <summary>
        /// Returns all remaining unresolved cards (for returning to deck on flee).
        /// Marks this room as fled so it cannot be fled again.
        /// </summary>
        public IReadOnlyList<CardSO> Flee()
        {
            HasFled = true;
            var remaining = cards.AsReadOnly();
            return remaining;
        }
    }
