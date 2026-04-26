using UnityEngine;

public static class ScoreCalculator
{
    public static int CalculateDeathScore(DungeonRoom room, DeckManager deckManager)
    {
        int monsterTotal = 0;
        
        foreach (var card in room.Cards)
        {
            if (card.Category == CardCategory.Monster)
                monsterTotal += card.Value;
        }
        
        foreach (var card in deckManager.DrawPile)
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
