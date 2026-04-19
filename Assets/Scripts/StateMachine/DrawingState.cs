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
/// Win condition: if fewer than 4 cards remain in the draw pile a full room can
/// never be dealt — the remaining cards (1-3) are dealt first, then discarded
/// after animations complete, triggering Victory.
/// </summary>
public class DrawingState : IGameState
{
    private const int RoomSize    = 4;
    private const int RefillCount = 3;

    private GameContext context;
    private bool isFinalDeal;

    public void Enter(GameContext context)
    {
        this.context = context;
        isFinalDeal = false;
        context.DungeonRoom.OnRoomReady += HandleRoomReady;

        // Win condition: fewer than 4 cards remain — deal whatever is left,
        // then trigger victory after animations complete.
        if (context.DeckManager.DrawPileCount < RoomSize)
        {
            isFinalDeal = true;
            int remaining = context.DeckManager.DrawPileCount;
            var finalCards = context.DeckManager.DrawMultiple(remaining);
            context.DungeonRoom.Deal(finalCards);
            Debug.Log($"[DrawingState] Final deal — {remaining} cards remaining. Victory after animations.");
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
        // If this was the final deal (fewer than 4 cards), trigger victory.
        if (isFinalDeal)
        {
            // Discard all remaining cards in the room.
            context.DeckManager.DiscardAll();
            
            int score = ScoreCalculator.CalculateVictoryScore(context.PlayerState, context.DungeonRoom);
            context.StateMachine.GetState<GameOverState>().SetResult(GameOverResult.Victory, score);
            context.StateMachine.TransitionTo<GameOverState>();
            return;
        }

        context.StateMachine.TransitionTo<PlayerChoiceState>();
    }
}
