using UnityEngine;

/// <summary>
/// Applies a card's effect to the game state, then decides what comes next:
/// - Room still has cards → back to PlayerChoiceState
/// - Room is cleared → DrawingState (next room)
/// - Player is dead → GameOverState
///
/// CombatResolver handles the actual damage math (step 4).
/// For now, monsters deal direct damage equal to their value.
/// </summary>
public class ResolvingState : IGameState
{
    private CardSO pendingCard;

    public void SetCard(CardSO card) => pendingCard = card;

    public void Enter(GameContext context)
    {
        if (pendingCard == null)
        {
            Debug.LogWarning("[ResolvingState] Entered with no card set.");
            return;
        }

        ResolveCard(context, pendingCard);
        pendingCard = null;
    }

    public void Exit(GameContext context) { }

    // ── Resolution logic ─────────────────────────────────────────────

    private void ResolveCard(GameContext context, CardSO card)
    {
        switch (card.Category)
        {
            case CardCategory.Monster:
                ResolveMonster(context, card);
                break;

            case CardCategory.Potion:
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

    private void ResolveMonster(GameContext context, CardSO monster)
    {
        // Stub: direct damage. CombatResolver replaces this in step 4.
        context.PlayerState.TakeDamage(monster.Value);
    }
}
