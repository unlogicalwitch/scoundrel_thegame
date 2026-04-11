using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles two scenarios:
///   1. Fresh room  — deck has cards, room is empty  → Deal() 4 cards.
///   2. Room refill — 3 cards were resolved, 1 survivor remains → RefillRoom() with 3 new cards.
///
/// In both cases the state waits for DungeonRoom.OnRoomReady (fired by RoomView
/// after all deal animations complete) before handing off to PlayerChoiceState.
///
/// If the deck is exhausted the game ends immediately.
/// </summary>
public class DrawingState : IGameState
{
    private const int RoomSize    = 4;
    private const int RefillCount = 3;

    private GameContext context;

    public void Enter(GameContext context)
    {
        this.context = context;
        context.DungeonRoom.OnRoomReady += HandleRoomReady;

        if (context.DeckManager.IsDeckEmpty)
        {
            context.StateMachine.TransitionTo<GameOverState>();
            return;
        }

        // If 1 card is still in the room (survivor from the previous round),
        // only draw 3 new cards and refill the empty slots.
        if (context.DungeonRoom.RemainingCards == 1)
        {
            var newCards = context.DeckManager.DrawMultiple(RefillCount);
            context.DungeonRoom.RefillRoom(newCards);
        }
        else
        {
            // Fresh room — draw a full hand of 4.
            var cards = context.DeckManager.DrawMultiple(RoomSize);
            context.DungeonRoom.Deal(cards);
        }
    }

    public void Exit(GameContext context)
    {
        context.DungeonRoom.OnRoomReady -= HandleRoomReady;
    }

    private void HandleRoomReady()
    {
        context.StateMachine.TransitionTo<PlayerChoiceState>();
    }
}
