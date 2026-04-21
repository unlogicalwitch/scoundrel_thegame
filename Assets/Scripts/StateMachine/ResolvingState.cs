using UnityEngine;

/// <summary>
/// Applies a card's effect to the game state, then routes to the next state:
///   - Player is dead          → GameOverState
///   - Room cleared (1 left)   → DrawingState  (refill with 3 new cards)
///   - Cards still in room     → PlayerChoiceState  (keep choosing)
///
/// Call SetCard(card, choice) before TransitionTo&lt;ResolvingState&gt;().
/// CombatResolver handles all monster damage math.
/// </summary>
public class ResolvingState : IGameState
{
    private CardSO pendingCard;
    private FightChoice pendingChoice;
    private GameContext context;

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
        context.DungeonRoom.SetRoomState(true);
        
        if (pendingCard == null)
        {
            Debug.LogWarning("[ResolvingState] Entered with no card set.");
            return;
        }
        this.context = context;

        var card   = pendingCard;
        var choice = pendingChoice;
        pendingCard   = null;
        pendingChoice = FightChoice.None;

        ResolveCard(context, card, choice);
        Route(context);
    }

    public void Exit(GameContext context)
    {
        // nothing to clean up
    }

    // ── Resolution ───────────────────────────────────────────────────

    private void ResolveCard(GameContext context, CardSO card, FightChoice choice)
    {
        switch (card.Category)
        {
            case CardCategory.Monster:
                CombatResolver.Resolve(context.PlayerState, card, choice);
                CameraShake.Shake(0.175f, 0.15f);
                break;

            case CardCategory.Potion:
                var gameSettings = ServiceLocator.Get<GameSettings>();
                bool canConsumePotion = gameSettings.ConsumeMultiplePotionAllowed || !context.DungeonRoom.PotionUsedThisRoom;
                
                if (canConsumePotion)
                    context.PlayerState.Heal(card.Value);
                break;

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
            int score = ScoreCalculator.CalculateDeathScore(context.DungeonRoom);
            context.StateMachine.GetState<GameOverState>().SetResult(GameOverResult.Death, score);
            context.StateMachine.TransitionTo<GameOverState>();
            return;
        }

        // 1 card remaining means the room was just cleared → refill via DrawingState
        if (context.DungeonRoom.RemainingCards == 1)
        {
            context.StateMachine.TransitionTo<DrawingState>();
            return;
        }

        context.StateMachine.TransitionTo<PlayerChoiceState>();
    }
}
