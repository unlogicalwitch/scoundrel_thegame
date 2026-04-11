using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 4 card slot positions in the dungeon room AND the weapon slot.
/// Listens to DungeonRoom events and instantiates / removes CardViews accordingly.
/// Listens to PlayerState.OnWeaponChanged to animate weapon cards in/out of the weapon slot.
/// Once all deal animations finish, calls DungeonRoom.NotifyRoomReady() so
/// DrawingState can transition to PlayerChoiceState.
/// Attach to a persistent GameObject in the Game scene.
/// </summary>
public class RoomView : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [Header("Layout")]
    [SerializeField] private Transform[] cardSlots;      // 4 empty GameObjects marking slot positions
    [SerializeField] private Transform deckPosition;     // where cards fly from
    [SerializeField] private Transform discardPosition;  // where resolved cards fly to
    [SerializeField] private Transform weaponSlot;       // where the equipped weapon card sits

    [Header("Prefabs & Assets")]
    [SerializeField] private GameObject cardPrefab;      // has CardView + SpriteRenderer + Collider2D
    [SerializeField] private Sprite cardBackSprite;

    // ── Runtime ──────────────────────────────────────────────────────

    private readonly List<CardView> activeCardViews = new(4);
    private DungeonRoom dungeonRoom;
    private PlayerChoiceState choiceState;
    private DragResolver dragResolver;
    private PlayerState playerState;

    // Weapon slot — at most one CardView lives here at a time
    private CardView equippedWeaponView;

    // ── Setup ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameStateMachine after context is ready.
    /// </summary>
    public void Initialise(DungeonRoom room, PlayerChoiceState choice, DragResolver resolver, PlayerState player)
    {
        dungeonRoom = room;
        choiceState = choice;
        dragResolver = resolver;
        playerState = player;

        dungeonRoom.OnRoomDealt    += HandleRoomDealt;
        dungeonRoom.OnCardResolved += HandleCardResolved;
        dungeonRoom.OnRoomFled     += HandleRoomFled;

        playerState.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDestroy()
    {
        if (dungeonRoom != null)
        {
            dungeonRoom.OnRoomDealt -= HandleRoomDealt;
            dungeonRoom.OnCardResolved -= HandleCardResolved;
            dungeonRoom.OnRoomFled -= HandleRoomFled;
        }

        if (playerState != null)
            playerState.OnWeaponChanged -= HandleWeaponChanged;
    }

    // ── Event handlers ──────────────────────────────────────────

    private void HandleRoomDealt(List<CardSO> cards)
    {
        ClearRoom();

        int totalCards = Mathf.Min(cards.Count, cardSlots.Length);
        int completedCount = 0;

        for (int i = 0; i < totalCards; i++)
        {
            var go = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
            var view = go.GetComponent<CardView>();
            
            // Cards initialisation
            view.Initialise(cards[i], choiceState, dragResolver, cardBackSprite);
            view.DealToSlot(cardSlots[i].position, i, onDealComplete: () =>
            {
                completedCount++;
                if (completedCount >= totalCards)
                    dungeonRoom.NotifyRoomReady();
            });

            activeCardViews.Add(view);
        }
    }

    private void HandleCardResolved(CardSO card)
    {
        var view = activeCardViews.Find(v => v != null);
        
        foreach (var cardView in activeCardViews)
        {
            if (cardView != null && cardView.CardData == card)
            {
                view = cardView;
                break;
            }
        }

        if (view == null) return;

        activeCardViews.Remove(view);
    }

    private void HandleRoomFled()
    {
        foreach (var view in activeCardViews)
            if (view != null)
            {
                view.Discard(deckPosition.position, () =>
                {
                    dungeonRoom.NotifyRoomReady();
                });
            }
        activeCardViews.Clear();
    }

    /// <summary>
    /// Called when PlayerState.OnWeaponChanged fires.
    /// newWeapon is null when the weapon is unequipped (e.g. bare-hand fight).
    /// </summary>
    private void HandleWeaponChanged(CardSO newWeapon)
    {
        var newView = activeCardViews.Find(v => v != null);
        
        foreach (var cardView in activeCardViews)
        {
            if (cardView != null && cardView.CardData == newWeapon)
            {
                newView = cardView;
                break;
            }
        }
        
        // Discard the old weapon card first, then show the new one (if any).
        if (equippedWeaponView != null)
        {
            var oldView = equippedWeaponView;
            equippedWeaponView = null;

            oldView.Discard(discardPosition.position, onDiscardComplete: () =>
            {
                newView.DealToSlot(weaponSlot.position, 0, onDealComplete: () => {});
                equippedWeaponView = newView;
            });
        }
        else
        {
            newView.MoveTo(weaponSlot.position);
            equippedWeaponView = newView;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private void ClearRoom()
    {
        foreach (var view in activeCardViews)
            if (view != null) Destroy(view.gameObject);

        activeCardViews.Clear();
    }
}
