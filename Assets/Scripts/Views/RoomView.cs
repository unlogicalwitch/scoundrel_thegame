using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 4 card slot positions in the dungeon room AND the weapon slot.
/// Listens to DungeonRoom events and instantiates / removes CardViews accordingly.
///
/// Two deal scenarios:
///   OnRoomDealt    — full fresh deal: clear all views, spawn 4 new cards.
///   OnRoomRefilled — 3 cards resolved, 1 survivor stays: spawn 3 new cards
///                    into the empty slots only; the surviving CardView is untouched.
///
/// Once all deal/refill animations finish, calls DungeonRoom.NotifyRoomReady()
/// so DrawingState can transition to PlayerChoiceState.
/// </summary>
public class RoomView : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [Header("Layout")]
    [SerializeField] private Transform[] cardSlots;
    [SerializeField] private Transform drawDeck;
    [SerializeField] private Transform discardDeck;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private float slainCardSpacing;

    [Header("Prefabs & Assets")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite cardBackSprite;

    // ── Runtime ──────────────────────────────────────────────────────

    // Index-aligned with cardSlots: null means the slot is empty.
    private readonly CardView[] slotViews = new CardView[4];

    private DungeonRoom dungeonRoom;
    private PlayerChoiceState choiceState;
    private DragResolver dragResolver;
    private PlayerState playerState;

    private CardView equippedWeaponView;

    // ── Setup ────────────────────────────────────────────────────────

    public void Initialise(DungeonRoom room, PlayerChoiceState choice, DragResolver resolver, PlayerState player)
    {
        dungeonRoom  = room;
        choiceState  = choice;
        dragResolver = resolver;
        playerState  = player;

        dungeonRoom.OnRoomDealt    += HandleRoomDealt;
        dungeonRoom.OnRoomRefilled += HandleRoomRefilled;
        dungeonRoom.OnCardResolved += HandleCardResolved;
        dungeonRoom.OnRoomFled     += HandleRoomFled;

        playerState.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDestroy()
    {
        if (dungeonRoom != null)
        {
            dungeonRoom.OnRoomDealt    -= HandleRoomDealt;
            dungeonRoom.OnRoomRefilled -= HandleRoomRefilled;
            dungeonRoom.OnCardResolved -= HandleCardResolved;
            dungeonRoom.OnRoomFled     -= HandleRoomFled;
        }

        if (playerState != null)
            playerState.OnWeaponChanged -= HandleWeaponChanged;
    }

    // ── Event handlers ───────────────────────────────────────────────

    /// <summary>
    /// Full fresh deal — clear everything and spawn 4 new cards.
    /// </summary>
    private void HandleRoomDealt(List<CardSO> cards)
    {
        ClearRoom();

        int total = Mathf.Min(cards.Count, cardSlots.Length);
        int completedCount = 0;

        for (int i = 0; i < total; i++)
        {
            var view = SpawnCard(cards[i], cardSlots[i].position, i, () =>
            {
                completedCount++;
                if (completedCount >= total)
                    dungeonRoom.NotifyRoomReady();
            });
            slotViews[i] = view;
        }
    }

    /// <summary>
    /// Refill — 1 survivor stays in its slot; animate 3 new cards into the empty slots.
    /// </summary>
    private void HandleRoomRefilled(List<CardSO> newCards)
    {
        // Collect the indices of empty slots (no surviving CardView).
        var emptySlots = new List<int>(3);
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (slotViews[i] == null)
                emptySlots.Add(i);
        }

        int total = Mathf.Min(newCards.Count, emptySlots.Count);
        int completedCount = 0;

        for (int j = 0; j < total; j++)
        {
            int slotIndex = emptySlots[j];
            var view = SpawnCard(newCards[j], cardSlots[slotIndex].position, j, () =>
            {
                completedCount++;
                if (completedCount >= total)
                    dungeonRoom.NotifyRoomReady();
            });
            slotViews[slotIndex] = view;
        }

        // Edge case: no empty slots (shouldn't happen in normal play).
        if (total == 0)
            dungeonRoom.NotifyRoomReady();
    }

    private void HandleCardResolved(CardSO card)
    {
        // Find the slot that holds this card.
        int slotIndex = -1;
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null && slotViews[i].CardData == card)
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex < 0) return;

        var view = slotViews[slotIndex];
        slotViews[slotIndex] = null;

        if (card.Category != CardCategory.Weapon)
        {
            view.Discard(discardDeck.position, () => { });
        }
    }

    private void HandleRoomFled()
    {
        for (int i = 0; i < slotViews.Length; i++)
        {
            var view = slotViews[i];
            if (view != null)
            {
                view.Discard(drawDeck.position, () =>
                {
                    dungeonRoom.NotifyRoomReady();
                });
                slotViews[i] = null;
            }
        }
    }

    private void HandleWeaponChanged(CardSO newWeapon)
    {
        Debug.Log("[RoomView] Weapon changed: " + newWeapon.Rank + " " + newWeapon.Suit);

        CardView newView = null;
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null && slotViews[i].CardData == newWeapon)
            {
                newView = slotViews[i];
                break;
            }
        }

        if (newView == null) return;

        if (equippedWeaponView != null)
        {
            var oldView = equippedWeaponView;
            equippedWeaponView = null;

            oldView.Discard(discardDeck.position, onDiscardComplete: () =>
            {
                newView.MoveTo(weaponSlot.position);
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

    private CardView SpawnCard(CardSO cardData, Vector3 targetSlotPosition, int dealIndex, System.Action onComplete)
    {
        var go   = Instantiate(cardPrefab, drawDeck.position, Quaternion.identity);
        var view = go.GetComponent<CardView>();
        view.Initialise(cardData, choiceState, dragResolver, cardBackSprite);
        view.DealToSlot(targetSlotPosition, dealIndex, onDealComplete: onComplete);
        return view;
    }

    private void ClearRoom()
    {
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null)
            {
                Destroy(slotViews[i].gameObject);
                slotViews[i] = null;
            }
        }

        // Also destroy the equipped weapon view if present.
        if (equippedWeaponView != null)
        {
            Destroy(equippedWeaponView.gameObject);
            equippedWeaponView = null;
        }
    }
}
