using UnityEngine;

public enum Suit
{
    Spades,     // Monsters
    Clubs,      // Monsters
    Hearts,     // Potions
    Diamonds,   // Weapons
    Joker       // Reserved — not used in core, available for expansion
}
 
public enum Rank
{
    Two   = 2,
    Three = 3,
    Four  = 4,
    Five  = 5,
    Six   = 6,
    Seven = 7,
    Eight = 8,
    Nine  = 9,
    Ten   = 10,
    Jack  = 11,
    Queen = 12,
    King  = 13,
    Ace   = 14,
    Joker = 0   // Reserved for special cards
}
 
public enum CardCategory
{
    Monster,    // Spades + Clubs
    Potion,     // Hearts
    Weapon,     // Diamonds
    Special     // Joker — reserved
}

public enum ZoneType
{
    None, 
    Weapon, 
    Barehanded,
    Use
}

public enum FightChoice
{
    None, 
    WithWeapon,
    Barehanded
}
