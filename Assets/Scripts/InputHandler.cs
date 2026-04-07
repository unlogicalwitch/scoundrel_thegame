using UnityEngine;

/// <summary>
/// Detects swipe gestures and forwards them to PlayerChoiceState.
/// Currently handles swipe down → flee.
/// Attach to a persistent GameObject in the Game scene.
/// Swaps cleanly to New Input System later without touching any game logic.
/// </summary>
public class InputHandler : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [Tooltip("Minimum pixel distance to count as a swipe.")]
    [SerializeField] private float swipeThreshold = 80f;

    // ── State ────────────────────────────────────────────────────────

    private PlayerChoiceState choiceState;
    private Vector2 touchStart;
    private bool tracking;

    // ── Setup ────────────────────────────────────────────────────────

    public void Initialise(PlayerChoiceState choice)
    {
        choiceState = choice;
    }

    // ── Update ───────────────────────────────────────────────────────

    private void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseSwipe();
        #else
        HandleTouchSwipe();
        #endif
    }

    // ── Mouse (editor / desktop) ──────────────────────────────────────

    private void HandleMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
            tracking   = true;
        }

        if (Input.GetMouseButtonUp(0) && tracking)
        {
            tracking = false;
            if (!IsPointerOverCard())
                EvaluateSwipe((Vector2)Input.mousePosition - touchStart);
        }
    }

    // ── Touch (Android / iOS) ────────────────────────────────────────

    private void HandleTouchSwipe()
    {
        if (Input.touchCount == 0) return;

        var touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStart = touch.position;
                tracking   = true;
                break;

            case TouchPhase.Ended when tracking:
                tracking = false;
                if (!IsPointerOverCard())
                    EvaluateSwipe(touch.position - touchStart);
                break;

            case TouchPhase.Canceled:
                tracking = false;
                break;
        }
    }

    // ── Gesture evaluation ───────────────────────────────────────────

    private void EvaluateSwipe(Vector2 delta)
    {
        if (delta.magnitude < swipeThreshold) return;

        // Only care about swipe down for now
        // Vertical dominates: |dy| > |dx| means it's more vertical than horizontal
        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && delta.y < 0)
        {
            Debug.Log("[InputHandler] flee input received" );
            choiceState?.RequestFlee();
        }
    }
    
    private bool IsPointerOverCard()
    {
        var hit = Physics2D.Raycast(
            Camera.main.ScreenToWorldPoint(Input.mousePosition), 
            Vector2.zero
        );
        return hit.collider != null && hit.collider.GetComponent<CardView>() != null;
    }
}
