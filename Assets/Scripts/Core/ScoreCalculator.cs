using UnityEngine;

/// <summary>
/// Stateless score math. No MonoBehaviour, no scene references.
///
/// Death score  — negative: sum of all remaining monster card values in the room at time of death.
/// Victory score — positive: player's remaining HP plus the value of any unresolved
///                 potion card still sitting in the last room.
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// Called when the player dies.
    /// Returns a negative integer equal to -(sum of monster values still in the room).
    /// </summary>
    public static int CalculateDeathScore(DungeonRoom room)
    {
        int monsterTotal = 0;
        foreach (var card in room.Cards)
        {
            if (card.Category == CardCategory.Monster)
                monsterTotal += card.Value;
        }
        return -monsterTotal;
    }

    /// <summary>
    /// Called when the deck is exhausted and the player wins.
    /// Returns player HP + value of any unresolved potion still in the room.
    /// </summary>
    public static int CalculateVictoryScore(PlayerState player, DungeonRoom room)
    {
        int potionBonus = 0;
        foreach (var card in room.Cards)
        {
            if (card.Category == CardCategory.Potion)
                potionBonus += card.Value;
        }
        return player.CurrentHealth + potionBonus;
    }
}
