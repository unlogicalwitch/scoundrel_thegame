using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the drop-zone UI elements at the bottom of the screen.
///
/// HOW THE FADE WORKS (UI Toolkit visibility pattern)
/// ────────────────────────────────────────────────────
/// UI Toolkit CSS transitions cannot animate `display` — it is binary (flex / none).
/// They CAN animate `opacity`. So we use a two-step approach:
///
///   SHOW  1. Set display:flex  (element enters the layout tree)
///         2. Next frame: add class "visible" → opacity transitions 0 → 1
///
///   HIDE  1. Remove class "visible" → opacity transitions 1 → 0
///         2. TransitionEndEvent fires → set display:none
///            (element leaves the layout tree only after the fade completes)
///
/// If we set display:none at the same time as opacity:0, the element is
/// removed from the layout tree immediately and the transition never plays.
///
/// Attach to the UIDocument GameObject in the Game scene.
/// </summary>
public class ZoneView : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────

    [SerializeField] private UIDocument uiDocument;

    // ── Elements ─────────────────────────────────────────────────────────

    private VisualElement zonesContainer;
    private VisualElement weaponZone;
    private VisualElement barehandZone;
    private VisualElement useZone;

    // ── USS class names ───────────────────────────────────────────────────

    private const string HiddenClass   = "hidden";
    private const string HighlightClass = "highlighted";

    // ── Setup ────────────────────────────────────────────────────────────

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        zonesContainer = root.Q<VisualElement>("ZonesContainer");
        weaponZone = root.Q<VisualElement>("WeaponZone");
        barehandZone = root.Q<VisualElement>("BarehandZone");
        useZone = root.Q<VisualElement>("UseZone");

        // When the fade-out transition finishes, complete the hide by
        // removing the element from the layout tree.
        zonesContainer.RegisterCallback<TransitionEndEvent>(OnContainerTransitionEnd);
    }

    private void Start()
    {
        // Ensure the container starts fully hidden (display:none is set in USS).
        // We call this to guarantee a clean state even if the USS default changes.
        zonesContainer.style.display = DisplayStyle.None;
        zonesContainer.AddToClassList(HiddenClass);
    }

    private void OnDestroy()
    {
        zonesContainer?.UnregisterCallback<TransitionEndEvent>(OnContainerTransitionEnd);
    }

    // ── Public API (called by DragResolver) ───────────────────────────────

    /// <summary>
    /// Show weapon + barehanded zones for a monster card.
    /// Only shows the weapon zone if the player has a valid weapon equipped.
    /// </summary>
    public void ShowMonsterZones(bool hasValidWeapon)
    {
        SetZoneVisible(useZone, false);
        SetZoneVisible(weaponZone, hasValidWeapon);
        SetZoneVisible(barehandZone, true);
        ShowContainer();
    }

    /// <summary>
    /// Show a single use zone for potions and weapon cards.
    /// </summary>
    public void ShowUseZone()
    {
        SetZoneVisible(weaponZone, false);
        SetZoneVisible(barehandZone, false);
        SetZoneVisible(useZone, true);
        ShowContainer();
    }

    /// <summary>
    /// Fade out and hide all zones. Call this after a drag ends.
    /// </summary>
    public void HideAll()
    {
        RemoveHighlightAll();
        HideContainer();
    }

    /// <summary>
    /// Returns which zone contains the given screen position, or ZoneType.None.
    /// Call this on finger/mouse release to determine the player's intent.
    /// </summary>
    public ZoneType GetZoneAt(Vector2 screenPosition)
    {
        var uiPos = ScreenToUIPosition(screenPosition);

        if (IsZoneVisible(weaponZone)   && weaponZone.worldBound.Contains(uiPos))
            return ZoneType.Weapon;

        if (IsZoneVisible(barehandZone) && barehandZone.worldBound.Contains(uiPos))
            return ZoneType.Barehanded;

        if (IsZoneVisible(useZone)      && useZone.worldBound.Contains(uiPos))
            return ZoneType.Use;

        return ZoneType.None;
    }

    /// <summary>
    /// Highlight the zone currently under the finger while dragging.
    /// Call this from CardView during drag for live feedback.
    /// </summary>
    public void UpdateHighlight(Vector2 screenPosition)
    {
        RemoveHighlightAll();

        switch (GetZoneAt(screenPosition))
        {
            case ZoneType.Weapon:
                weaponZone.AddToClassList(HighlightClass);
                break;
            case ZoneType.Barehanded:
                barehandZone.AddToClassList(HighlightClass);
                break;
            case ZoneType.Use:
                useZone.AddToClassList(HighlightClass);
                break;
        }
    }

    // ── Container show / hide (two-step) ─────────────────────────────────

    private void ShowContainer()
    {
        // Step 1: make the element part of the layout tree so it has a size.
        zonesContainer.style.display = DisplayStyle.Flex;

        // Step 2: on the very next frame, add the class that transitions opacity
        // from 0 → 1. We defer by one frame so the browser-style layout pass
        // has already measured the element at opacity:0 before we change it.
        zonesContainer.schedule.Execute(() =>
            zonesContainer.RemoveFromClassList(HiddenClass));
    }

    private void HideContainer()
    {
        // Step 1: remove the class → opacity transitions 1 → 0.
        // Step 2 happens in OnContainerTransitionEnd (sets display:none).
        zonesContainer.AddToClassList(HiddenClass);
    }

    /// <summary>
    /// Called automatically when any CSS transition on ZonesContainer ends.
    /// We use this to set display:none only AFTER the fade-out completes.
    /// </summary>
    private void OnContainerTransitionEnd(TransitionEndEvent evt)
    {
        // Only act when the opacity transition finishes and the container
        // is now invisible (i.e. we were hiding, not showing).
        if (zonesContainer.ClassListContains(HiddenClass))
        {
            zonesContainer.style.display = DisplayStyle.None;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static void SetZoneVisible(VisualElement zone, bool visible)
    {
        if (zone == null) return;
        zone.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static bool IsZoneVisible(VisualElement zone)
    {
        return zone != null && zone.style.display != DisplayStyle.None;
    }

    private void RemoveHighlightAll()
    {
        weaponZone?.RemoveFromClassList(HighlightClass);
        barehandZone?.RemoveFromClassList(HighlightClass);
        useZone?.RemoveFromClassList(HighlightClass);
    }

    /// <summary>
    /// Convert Unity screen position (origin bottom-left, Y up)
    /// to UI Toolkit panel position (origin top-left, Y down).
    /// worldBound values are in panel pixels, so we must scale by the same
    /// factor the panel uses to map its reference resolution onto the screen.
    /// </summary>
    private Vector2 ScreenToUIPosition(Vector2 screenPos)
    {
        var root = uiDocument.rootVisualElement;
        // root.layout gives the panel's logical size (in panel pixels).
        // Screen.width/height gives the actual screen size.
        // The ratio is the scale factor applied by the Panel Settings scale mode.
        float scaleX = root.layout.width  / Screen.width;
        float scaleY = root.layout.height / Screen.height;
        return new Vector2(screenPos.x * scaleX, (Screen.height - screenPos.y) * scaleY);
    }
}
