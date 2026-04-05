using UnityEngine;

/// <summary>
/// Returns all unresolved room cards to the bottom of the deck,
/// then moves to DrawingState for a fresh room.
/// The room marks itself as fled so this can't be used twice.
/// </summary>
public class FleeState : IGameState
{
    private GameContext context;
    
    public void Enter(GameContext context)
    {
        this.context = context;
        context.DungeonRoom.OnRoomReady += HandleRoomReady;
        
        var remainingCards = context.DungeonRoom.Flee();
        context.DeckManager.ReturnToDeck((System.Collections.Generic.IList<CardSO>)remainingCards);
    }

    public void Exit(GameContext context)
    {
        context.DungeonRoom.OnRoomReady -= HandleRoomReady;
    }
    
    private void HandleRoomReady()
    {
        context.StateMachine.TransitionTo<DrawingState>();
    } 
}
