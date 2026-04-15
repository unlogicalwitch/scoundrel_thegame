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
/// never be dealt — the leftovers are discarded and the run ends as a Victory.
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

        // Win condition: fewer than 4 cards remain — a full room can never be dealt.
        // Discard the leftovers and end the run as a victory.
        if (context.DeckManager.DrawPileCount < RoomSize)
        {
            context.DungeonRoom.OnRoomReady -= HandleRoomReady;
            context.DeckManager.DiscardAll();
            int score = ScoreCalculator.CalculateVictoryScore(context.PlayerState, context.DungeonRoom);
            context.StateMachine.GetState<GameOverState>().SetResult(GameOverResult.Victory, score);
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
