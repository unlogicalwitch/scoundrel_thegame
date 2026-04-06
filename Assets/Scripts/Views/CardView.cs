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
    /// Called by RoomView immediately after instantiation.
    /// </summary>
    public void Initialise(CardSO data, PlayerChoiceState choice, Sprite backSprite)
    {
        cardData = data;
        choiceState = choice;
        isInteractable = false;

        spriteRenderer.sprite = backSprite;
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        transform.position = worldPos;
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

    public void Snapback(Vector3 targetPosition)
    {
        
    }

    // ── Input ────────────────────────────────────────────────────────

    private void OnMouseDown()
    {
        if (!isInteractable) return;
        isDragging = true;
        //dragResolver?.OnDragStarted(this);
    }

    private void OnMouseEnter()
    {
        Debug.Log($"[CardView] Mouse enter: {gameObject.name}");
        
        if (!isInteractable) return;
        animator.PlayHoverEnter();
    }
    
    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        //dragResolver?.OnDragReleased(this, Input.mousePosition);
    }

    private void OnMouseExit()
    {
        if (!isInteractable) return;
        animator.PlayHoverExit(slotPosition);
    }

    // ── Cleanup ──────────────────────────────────────────────────────

    private void OnDestroy() => animator.Kill();
}
