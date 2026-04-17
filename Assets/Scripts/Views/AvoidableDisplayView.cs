using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives the flee-availability icon (AvoidableDisplayIcon).
/// Darkens the sprite when the player can no longer flee the current room,
/// and restores it when a new room is ready (flee is available again).
///
/// Flee is only valid while all 4 cards are still untouched (RemainingCards == 4).
/// Any card resolution or a new room deal/refill resets the state.
///
/// Wiring:
///   RoomView calls Initialise(dungeonRoom) after building its own references.
///   This view never reads GameContext directly.
/// </summary>
public class AvoidableDisplayView : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────

    [SerializeField] private SpriteRenderer iconRenderer;

    [Tooltip("Alpha multiplier applied to the icon color when fleeing is blocked.")]
    [SerializeField] [Range(0f, 1f)] private float blockedAlpha = 0.35f;

    // ── State ─────────────────────────────────────────────────────────

    private DungeonRoom dungeonRoom;
    private Color baseColor;

    // ── Setup ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by RoomView after its own Initialise completes.
    /// </summary>
    public void Initialise(DungeonRoom room)
    {
        dungeonRoom = room;
        baseColor  = iconRenderer.color;

        dungeonRoom.OnCardResolved += HandleCardResolved;
        dungeonRoom.OnRoomDealt    += HandleRoomReady;
        dungeonRoom.OnRoomRefilled += HandleRoomReady;
        dungeonRoom.OnRoomFled     += HandleRoomFled;

        // Sync to current state immediately (safe default: available).
        SetAvailable(true);
    }

    private void OnDestroy()
    {
        if (dungeonRoom == null) return;

        dungeonRoom.OnCardResolved -= HandleCardResolved;
        dungeonRoom.OnRoomDealt    -= HandleRoomReady;
        dungeonRoom.OnRoomRefilled -= HandleRoomReady;
        dungeonRoom.OnRoomFled     -= HandleRoomFled;
    }

    // ── Event handlers ────────────────────────────────────────────────

    private void HandleCardResolved(CardSO _)
    {
        // Once any card is touched, flee is no longer available.
        SetAvailable(false);
    }

    private void HandleRoomReady(List<CardSO> _)
    {
        if (dungeonRoom.FledLastRoom) return;
        
        SetAvailable(true);
    }

    private void HandleRoomFled()
    {
        SetAvailable(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private void SetAvailable(bool available)
    {
        if (iconRenderer == null) return;

        var c = baseColor;
        c.a = available ? baseColor.a : baseColor.a * blockedAlpha;

        // Also darken RGB slightly so it reads as "disabled" even on dark backgrounds.
        float brightness = available ? 1f : 0.45f;
        iconRenderer.color = new Color(c.r * brightness, c.g * brightness, c.b * brightness, c.a);
    }
}
