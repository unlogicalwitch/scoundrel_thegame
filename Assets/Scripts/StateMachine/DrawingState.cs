using UnityEngine;

/// <summary>
/// Deals 4 cards into the dungeon room, then immediately hands off
/// to PlayerChoiceState. If the deck is exhausted here the game ends.
/// </summary>
public class DrawingState : IGameState
{
    private const int RoomSize = 4;
    private GameContext context;
 
    public void Enter(GameContext context)
    {
        this.context = context;
        context.DungeonRoom.OnRoomReady += HandleRoomReady;
        
        if (context.DeckManager.IsDeckEmpty)
        {
            // No cards left — trigger game over
            context.PlayerState.TakeDamage(context.PlayerState.CurrentHealth);
            return;
        }
 
        var cards = context.DeckManager.DrawMultiple(RoomSize);
        context.DungeonRoom.Deal(cards);
    }
 
    public void Exit(GameContext context) { }

    private void HandleRoomReady()
    {
        context.StateMachine.TransitionTo<PlayerChoiceState>();
    }
}
