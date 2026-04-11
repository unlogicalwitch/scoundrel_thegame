using UnityEngine;

/// <summary>
    /// Sits between CardView (drag input) and PlayerChoiceState (game logic).
    /// On drag start: tells ZoneView which zones to show for this card type.
    /// On drag release: checks which zone was hit and triggers the right action.
    /// If no zone hit: tells CardView to snap back to its slot.
    /// Attach to a persistent GameObject in the Game scene.
    /// </summary>
    public class DragResolver : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────
 
        [SerializeField] private ZoneView zoneView;
 
        // ── Runtime ──────────────────────────────────────────────────────
 
        private PlayerChoiceState choiceState;
        private PlayerState playerState;
 
        // ── Setup ────────────────────────────────────────────────────────
 
        public void Initialise(PlayerChoiceState choice, PlayerState player)
        {
            choiceState = choice;
            playerState = player;
        }
 
        // ── Drag API (called by CardView) ─────────────────────────────────
 
        /// <summary>
        /// Called by CardView when the player starts dragging a card.
        /// Tells ZoneView which zones are relevant for this card.
        /// </summary>
        public void OnDragStarted(CardView card)
        {
            if (choiceState == null) return;
            
            switch (card.CardData.Category)
            {
                case CardCategory.Monster:
                    Debug.Log("[DragResolver] Card value: " + card.CardData.Value);
                    zoneView.ShowMonsterZones(card.CardData.Value < playerState.WeaponDurability);
                    break;
 
                case CardCategory.Potion:
                case CardCategory.Weapon:
                    zoneView.ShowUseZone();
                    break;
            }
        }
 
        /// <summary>
        /// Called by CardView every frame while dragging.
        /// Updates the highlight on whichever zone is currently under the pointer.
        /// </summary>
        public void OnDragUpdated(Vector2 screenPosition)
        {
            zoneView.UpdateHighlight(screenPosition);
        }

        /// <summary>
        /// Called by CardView on finger release.
        /// Checks which zone was hit and triggers the corresponding action,
        /// or snaps the card back if no zone was hit.
        /// </summary>
        public void OnDragReleased(CardView card, Vector2 screenPosition)
        {
            var hitZone = zoneView.GetZoneAt(screenPosition);
            zoneView.HideAll();
 
            switch (hitZone)
            {
                case ZoneType.Weapon:
                    Debug.Log("[DragResolver] Attacked with weapon");
                    choiceState.SelectCard(card.CardData, FightChoice.WithWeapon);
                    break;
            
                case ZoneType.Barehanded:
                    Debug.Log("[DragResolver] Attacked with barehand");
                    choiceState.SelectCard(card.CardData, FightChoice.Barehanded);
                    break;
            
                case ZoneType.Use:
                    choiceState.SelectCard(card.CardData, FightChoice.None);
                    break;
            
                case ZoneType.None:
                    card.SnapBack();
                    break;
            }
        }
    }
