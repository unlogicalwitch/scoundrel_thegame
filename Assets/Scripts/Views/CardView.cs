using System;
using UnityEngine;

/// <summary>
/// Visual + input representation of a single card in the scene.
/// Owned by RoomView — one instance per card in the active room.
///
/// Responsibilities:
///   - Display the correct sprite (face / back)
///   - Play deal, flip, discard, and hover animations via CardAnimator
///   - Detect player tap and forward to PlayerChoiceState
///
/// Does NOT read or mutate any game state directly.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class CardView : MonoBehaviour
{
    // ── State ────────────────────────────────────────────────────────

    private CardSO cardData;
    private CardAnimator animator;
    private SpriteRenderer spriteRenderer;
    private DragResolver dragResolver;
    private Vector3 slotPosition;
    private bool isInteractable;
    private bool isDragging;

    public CardSO CardData => cardData;

    // ── References set by RoomView ────────────────────────────────────

    private PlayerChoiceState choiceState;
    private Camera mainCamera;

    // ── Setup ────────────────────────────────────────────────────────

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = new CardAnimator(transform, spriteRenderer);
    }

    /// <summary>
    /// Called by RoomView immediately after instantiation
    /// </summary>
    public void Initialise(CardSO data, PlayerChoiceState choice, DragResolver dragResolver, Sprite backSprite)
    {
        cardData = data;
        choiceState = choice;
        this.dragResolver = dragResolver;
        isInteractable = false;

        spriteRenderer.sprite = backSprite;
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        transform.position = worldPos;

        dragResolver?.OnDragUpdated(Input.mousePosition);
    }

    /// <summary>
    /// Deal this card to a world-space slot position, then flip face-up.
    /// Called by RoomView after all cards are instantiated.
    /// </summary>
    /// <param name="onDealComplete">Invoked once the full deal+flip animation finishes.</param>
    public void DealToSlot(Vector3 targetSlot, int slotIndex, System.Action onDealComplete)
    {
        slotPosition = targetSlot;

        animator.PlayDeal(targetSlot, slotIndex, onComplete: () =>
        {
            animator.PlayFlip(cardData.FrontSprite, onComplete: () =>
            {
                isInteractable = true;
                onDealComplete?.Invoke();
            });
        });
    }

    public void MoveTo(Vector3 targetPosition)
    {
        slotPosition = targetPosition;

        animator.PlayDeal(targetPosition, 0);
    }

    /// <summary>
    /// Play the discard animation then destroy this GameObject.
    /// Called by RoomView when the card is resolved.
    /// </summary>
    public void Discard(Vector3 discardPosition, System.Action onDiscardComplete)
    {
        isInteractable = false;
        animator.PlayDiscard(discardPosition, onComplete: () =>
        {
            onDiscardComplete?.Invoke();
            Destroy(gameObject);
        });
    }

    public void SnapBack()
    {
        isInteractable = false;
        animator.PlaySnapBack(slotPosition, onComplete: () =>
        {
            isInteractable = true;
        });
    }

    // ── Input ────────────────────────────────────────────────────────

    private void OnMouseDown()
    {
        Debug.Log($"[CardView] Mouse Down: {gameObject.name}");
        if (!isInteractable) return;
        
        dragResolver?.OnDragStarted(this);
        isDragging = true;
    }
    
    private void OnMouseUp()
    {
        Debug.Log($"[CardView] Mouse Up: {gameObject.name}");
        if (!isDragging) return;
        
        dragResolver?.OnDragReleased(this, Input.mousePosition);
        isDragging = false;
    }

    // ── Cleanup ──────────────────────────────────────────────────────

    private void OnDestroy() => animator.Kill();
}
