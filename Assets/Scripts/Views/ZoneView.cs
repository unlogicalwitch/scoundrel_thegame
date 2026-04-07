using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
    /// Manages the drop zone UI elements at the bottom of the screen.
    /// Zones are hidden at rest and appear when a card drag starts.
    /// Uses USS transitions for fade in/out.
    /// Attach to the UIDocument GameObject in the Game scene.
    /// </summary>
    public class ZoneView : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────
 
        [SerializeField] private UIDocument uiDocument;
 
        // ── Elements ─────────────────────────────────────────────────────
 
        private VisualElement zonesContainer;
        private VisualElement weaponZone;
        private VisualElement barehandZone;
        private VisualElement useZone;
 
        // ── USS class names ───────────────────────────────────────────────
 
        private const string VisibleClass    = "visible";
        private const string HighlightClass  = "highlighted";
 
        // ── Setup ────────────────────────────────────────────────────────
 
        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
 
            zonesContainer = root.Q<VisualElement>("zones-container");
            weaponZone     = root.Q<VisualElement>("weapon-zone");
            barehandZone   = root.Q<VisualElement>("bare-zone");
            useZone        = root.Q<VisualElement>("use-zone");
        }
 
        // ── Public API (called by DragResolver) ───────────────────────────
 
        /// <summary>
        /// Show weapon + barehanded zones for a monster card.
        /// Only shows weapon zone if the player has a valid weapon.
        /// </summary>
        public void ShowMonsterZones(bool hasValidWeapon)
        {
            SetZoneVisible(useZone, false);
            SetZoneVisible(weaponZone, hasValidWeapon);
            SetZoneVisible(barehandZone, true);
            SetContainerVisible(true);
        }
 
        /// <summary>
        /// Show a single use zone for potions and weapon cards.
        /// </summary>
        public void ShowUseZone()
        {
            SetZoneVisible(weaponZone, false);
            SetZoneVisible(barehandZone, false);
            SetZoneVisible(useZone, true);
            SetContainerVisible(true);
        }
 
        /// <summary>
        /// Hide all zones. Called after drag ends regardless of outcome.
        /// </summary>
        public void HideAll()
        {
            SetContainerVisible(false);
            RemoveHighlightAll();
        }
 
        /// <summary>
        /// Check which zone contains the given screen position.
        /// Called by DragResolver on finger release.
        /// </summary>
        public ZoneType GetZoneAt(Vector2 screenPosition)
        {
            var uiPos = ScreenToUIPosition(screenPosition);
 
            if (weaponZone.visible && weaponZone.worldBound.Contains(uiPos))
                return ZoneType.Weapon;
 
            if (barehandZone.visible && barehandZone.worldBound.Contains(uiPos))
                return ZoneType.Barehanded;
 
            if (useZone.visible && useZone.worldBound.Contains(uiPos))
                return ZoneType.Use;
 
            return ZoneType.None;
        }
 
        /// <summary>
        /// Highlight the zone under the finger while dragging.
        /// Call this from CardView.Update() during drag for live feedback.
        /// </summary>
        public void UpdateHighlight(Vector2 screenPosition)
        {
            RemoveHighlightAll();
 
            var uiPos    = ScreenToUIPosition(screenPosition);
            var hovered  = GetZoneAt(screenPosition);
 
            switch (hovered)
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
 
        // ── Helpers ──────────────────────────────────────────────────────
 
        private void SetContainerVisible(bool visible)
        {
            if (visible)
                zonesContainer.AddToClassList(VisibleClass);
            else
                zonesContainer.RemoveFromClassList(VisibleClass);
        }
 
        private void SetZoneVisible(VisualElement zone, bool visible)
        {
            if (zone == null) return;
            zone.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
 
        private void RemoveHighlightAll()
        {
            weaponZone?.RemoveFromClassList(HighlightClass);
            barehandZone?.RemoveFromClassList(HighlightClass);
            useZone?.RemoveFromClassList(HighlightClass);
        }
 
        /// <summary>
        /// Convert Unity screen position (origin bottom-left, y up)
        /// to UI Toolkit position (origin top-left, y down).
        /// </summary>
        private Vector2 ScreenToUIPosition(Vector2 screenPos)
        {
            return new Vector2(screenPos.x, Screen.height - screenPos.y);
        }
    }
