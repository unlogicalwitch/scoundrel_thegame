using UnityEngine;

/// <summary>
/// Applies a card's effect to the game state, then routes to the next state:
///   - Player is dead          → GameOverState
///   - Room is cleared         → DrawingState  (deal a fresh room)
///   - Cards still in room     → PlayerChoiceState  (keep choosing)
///
/// Call SetCard(card, choice) before TransitionTo&lt;ResolvingState&gt;().
/// CombatResolver handles all monster damage math.
/// </summary>
public class ResolvingState : IGameState
{
    private CardSO pendingCard;
    private FightChoice pendingChoice;

    /// <summary>
    /// Called by PlayerChoiceState before transitioning here.
    /// </summary>
    public void SetCard(CardSO card, FightChoice choice)
    {
        pendingCard   = card;
        pendingChoice = choice;
    }

    public void Enter(GameContext context)
    {
        if (pendingCard == null)
        {
            Debug.LogWarning("[ResolvingState] Entered with no card set.");
            return;
        }

        var card   = pendingCard;
        var choice = pendingChoice;
        pendingCard   = null;
        pendingChoice = FightChoice.None;

        ResolveCard(context, card, choice);
        Route(context);
    }

    public void Exit(GameContext context) { }

    // ── Resolution ───────────────────────────────────────────────────

    private void ResolveCard(GameContext context, CardSO card, FightChoice choice)
    {
        switch (card.Category)
        {
            // triggers fight
            case CardCategory.Monster:
                CombatResolver.Resolve(context.PlayerState, card, choice);
                break;
            
            // triggers heal
            case CardCategory.Potion:
                context.PlayerState.Heal(card.Value);
                break;
            
            // triggers weapon equip
            case CardCategory.Weapon:
                context.PlayerState.EquipWeapon(card);
                break;

            case CardCategory.Special:
                Debug.Log($"[ResolvingState] Special card {card} — not implemented yet.");
                break;
        }

        context.DungeonRoom.Resolve(card);
        context.DeckManager.Discard(card);
    }

    // ── Routing ──────────────────────────────────────────────────────

    private void Route(GameContext context)
    {
        if (context.PlayerState.IsDead)
        {
            context.StateMachine.TransitionTo<GameOverState>();
            return;
        }

        if (context.DungeonRoom.IsCleared)
        {
            context.StateMachine.TransitionTo<DrawingState>();
            return;
        }

        context.StateMachine.TransitionTo<PlayerChoiceState>();
    }
}
