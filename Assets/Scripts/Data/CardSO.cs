using UnityEngine;

/// <summary>
    /// Immutable data definition for a single card.
    /// One asset per card — 44 assets total for the standard Scoundrel deck.
    /// Suits drive category; rank drives value. No runtime state lives here.
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Scoundrel/Card")]
    public class CardSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private Suit suit;
        [SerializeField] private Rank rank;
 
        [Header("Visuals")]
        [SerializeField] private Sprite frontSprite;
        [SerializeField] private Sprite backSprite;
 
        // ── Public read-only API ─────────────────────────────────────────
 
        public Suit Suit => suit;
        public Rank Rank => rank;
        public Sprite FrontSprite => frontSprite;
        public Sprite BackSprite => backSprite;
 
        /// <summary>
        /// Derived from suit. Spades/Clubs = Monster, Hearts = Potion,
        /// Diamonds = Weapon, Joker = Special (reserved).
        /// </summary>
        public CardCategory Category => suit switch
        {
            Suit.Spades   => CardCategory.Monster,
            Suit.Clubs    => CardCategory.Monster,
            Suit.Hearts   => CardCategory.Potion,
            Suit.Diamonds => CardCategory.Weapon,
            Suit.Joker    => CardCategory.Special,
            _             => CardCategory.Special
        };
 
        /// <summary>
        /// Numeric value used by combat and potion resolution.
        /// Matches rank integer value (2–14). Joker returns 0.
        /// </summary>
        public int Value => (int)rank;
 
        /// <summary>
        /// True if this card should be excluded from the standard
        /// 44-card Scoundrel deck (red Aces + face cards of Hearts/Diamonds).
        /// </summary>
        public bool IsRemovedFromScoundrelDeck =>
            suit == Suit.Joker ||
            (rank == Rank.Ace   && (suit == Suit.Hearts || suit == Suit.Diamonds)) ||
            (rank == Rank.Jack  && (suit == Suit.Hearts || suit == Suit.Diamonds)) ||
            (rank == Rank.Queen && (suit == Suit.Hearts || suit == Suit.Diamonds)) ||
            (rank == Rank.King  && (suit == Suit.Hearts || suit == Suit.Diamonds));
 
        public override string ToString() => $"{rank} of {suit} [{Category}, val={Value}]";
    }
