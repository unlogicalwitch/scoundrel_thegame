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
 
        private readonly List<CardSO> cards = new(4);
        private readonly List<CardSO> resolvedCards = new(4);
 
        // ── Events ───────────────────────────────────────────────────────
 
        public event Action<CardSO> OnCardResolved;
        public event Action<List<CardSO>> OnRoomDealt;
        public event Action OnRoomCleared;
        public event Action OnRoomReady;
        public event Action OnRoomFled;
 
        // ── Public API ───────────────────────────────────────────────────
 
        public IReadOnlyList<CardSO> Cards => cards.AsReadOnly();
        public bool FledLastRoom { get; private set; }
        public bool IsCleared => cards.Count == 0;
        public int RemainingCards => cards.Count;
 
        /// <summary>
        /// Set up a new room with up to 4 cards drawn from the deck.
        /// </summary>
        public void Deal(List<CardSO> newCards)
        {
            cards.Clear();
            resolvedCards.Clear();
            //FledLastRoom = false;
 
            foreach (var card in newCards)
                if (card != null) cards.Add(card);
 
            OnRoomDealt?.Invoke(cards);
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
        /// Called by RoomView once all deal animations have finished.
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
            return cards.AsReadOnly();
        }
    }
