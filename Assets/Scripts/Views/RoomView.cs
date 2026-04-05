using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 4 card slot positions in the dungeon room.
/// Listens to DungeonRoom events and instantiates / removes CardViews accordingly.
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

    [Header("Prefabs & Assets")]
    [SerializeField] private GameObject cardPrefab;      // has CardView + SpriteRenderer + Collider2D
    [SerializeField] private Sprite cardBackSprite;

    // ── Runtime ──────────────────────────────────────────────────────

    private readonly List<CardView> activeCardViews = new(4);
    private DungeonRoom dungeonRoom;
    private PlayerChoiceState choiceState;

    // ── Setup ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameStateMachine after context is ready.
    /// </summary>
    public void Initialise(DungeonRoom room, PlayerChoiceState choice)
    {
        dungeonRoom = room;
        choiceState = choice;

        dungeonRoom.OnRoomDealt += HandleRoomDealt;
        dungeonRoom.OnCardResolved += HandleCardResolved;
        dungeonRoom.OnRoomFled += HandleRoomFled;
    }

    private void OnDestroy()
    {
        if (dungeonRoom == null) return;
        dungeonRoom.OnRoomDealt -= HandleRoomDealt;
        dungeonRoom.OnCardResolved -= HandleCardResolved;
        dungeonRoom.OnRoomFled -= HandleRoomFled;
    }

    // ── Event handlers ───────────────────────────────────────────────

    private void HandleRoomDealt(List<CardSO> cards)
    {
        ClearRoom();

        int totalCards = Mathf.Min(cards.Count, cardSlots.Length);
        int completedCount = 0;

        for (int i = 0; i < totalCards; i++)
        {
            var go   = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
            var view = go.GetComponent<CardView>();

            view.Initialise(cards[i], choiceState, cardBackSprite);
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

        // Match by CardSO — find the view showing this card
        foreach (var cv in activeCardViews)
        {
            if (cv != null && cv.CardData == card)
            {
                view = cv;
                break;
            }
        }

        if (view == null) return;

        activeCardViews.Remove(view);
        view.Discard(discardPosition.position, () => {});
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

    // ── Helpers ──────────────────────────────────────────────────────

    private void ClearRoom()
    {
        foreach (var view in activeCardViews)
            if (view != null) Destroy(view.gameObject);

        activeCardViews.Clear();
    }
}
