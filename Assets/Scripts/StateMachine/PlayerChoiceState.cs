using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Waits for the player to tap a card or choose to flee.
/// The view layer calls SelectCard() or Flee() in response to input.
/// This state does not resolve anything — it only validates and records intent.
/// </summary>
public class PlayerChoiceState : IGameState
{
    // ── State ────────────────────────────────────────────────────────
 
    private GameContext context;
 
    // ── IGameState ───────────────────────────────────────────────────
 
    public void Enter(GameContext ctx)
    {
        context = ctx;
    }
 
    public void Exit(GameContext ctx)
    {
        context = null;
    }
 
    // ── Input API (called by InputHandler / CardView) ─────────────────
 
    /// <summary>
    /// Call this when the player taps a card in the room.
    /// </summary>
    public void SelectCard(CardSO card)
    {
        if (context == null) return;
        if (!context.DungeonRoom.Cards.Contains(card)) return;
 
        context.StateMachine.TransitionTo<ResolvingState>();
    }
 
    /// <summary>
    /// Call this when the player taps the flee button.
    /// Only valid if the room has not been fled before.
    /// </summary>
    public void RequestFlee()
    {
        if (context == null) return;
        
        // can only flee without if haven't interact with any cards yet and flee the previous room
        if (context.DungeonRoom.RemainingCards < 4) return; 
        if (context.DungeonRoom.FledLastRoom) return;

        Debug.Log("attempt fleeing");
        context.StateMachine.TransitionTo<FleeState>();
    }
}
