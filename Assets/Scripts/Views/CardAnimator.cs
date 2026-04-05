using DG.Tweening;
using UnityEngine;

/// <summary>
    /// All DOTween sequences for a single card live here.
    /// CardView owns one of these and calls it by name — no Tween logic leaks into CardView.
    /// </summary>
    public class CardAnimator
    {
        // ── Config ───────────────────────────────────────────────────────
 
        private static readonly float dealDuration = 0.35f;
        private static readonly float flipDuration = 0.15f;
        private static readonly float discardDuration = 0.25f;
        private static readonly float hoverHeight = 0f;
        private static readonly Ease dealEase = Ease.OutCubic;
        private static readonly Ease discardEase = Ease.InCubic;
 
        // ── References ───────────────────────────────────────────────────
 
        private readonly Transform    cardTransform;
        private readonly SpriteRenderer spriteRenderer;
 
        private Tween activeTween;
 
        public CardAnimator(Transform transform, SpriteRenderer renderer)
        {
            cardTransform = transform;
            spriteRenderer = renderer;
        }
 
        // ── Public API ───────────────────────────────────────────────────
 
        /// <summary>
        /// Arc the card from its current position to a target slot, then call onComplete.
        /// </summary>
        public void PlayDeal(Vector3 targetPosition, int slotIndex, System.Action onComplete = null)
        {
            Kill();
 
            // Slight stagger per slot so cards don't all land simultaneously
            float delay = slotIndex * 0.08f;
 
            // Waypoint arc: lift slightly above midpoint for a natural arc feel
            Vector3 start = cardTransform.position;
            Vector3 mid   = Vector3.Lerp(start, targetPosition, 0.5f) + Vector3.up * hoverHeight;
 
            activeTween = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(cardTransform.DOPath(
                    new[] { mid, targetPosition },
                    dealDuration,
                    PathType.CatmullRom)
                    .SetEase(dealEase))
                .AppendCallback(() => onComplete?.Invoke())
                .Play();
        }
 
        /// <summary>
        /// Flip the card face-up by scaling X to 0, swapping the sprite, then scaling back.
        /// </summary>
        public void PlayFlip(Sprite faceSprite, System.Action onComplete = null)
        {
            Kill();
 
            activeTween = DOTween.Sequence()
                .Append(cardTransform.DOScaleX(0f, flipDuration).SetEase(Ease.InSine))
                .AppendCallback(() => spriteRenderer.sprite = faceSprite)
                .Append(cardTransform.DOScaleX(1f, flipDuration).SetEase(Ease.OutSine))
                .AppendCallback(() => onComplete?.Invoke())
                .Play();
        }
 
        /// <summary>
        /// Slide the card to the discard position, fade out, then call onComplete.
        /// </summary>
        public void PlayDiscard(Vector3 discardPosition, System.Action onComplete = null)
        {
            Kill();
 
            activeTween = DOTween.Sequence()
                .Append(cardTransform.DOMove(discardPosition, discardDuration))
                .Join(spriteRenderer.DOFade(0.0f, discardDuration))
                .AppendCallback(() => onComplete?.Invoke())
                .Play();
        }
 
        /// <summary>
        /// Hover the card up slightly when the player's finger is over it.
        /// </summary>
        public void PlayHoverEnter()
        {
            Kill();
            activeTween = cardTransform
                .DOMoveY(cardTransform.position.y + 1f * 0.4f, 0.1f)
                .SetEase(Ease.OutSine);
        }
 
        /// <summary>
        /// Return the card to its slot position.
        /// </summary>
        public void PlayHoverExit(Vector3 slotPosition)
        {
            Kill();
            activeTween = cardTransform
                .DOMove(slotPosition, 0.1f)
                .SetEase(Ease.OutSine);
        }
 
        /// <summary>
        /// Punch the card scale briefly — use for invalid tap feedback.
        /// </summary>
        public void PlayInvalidFeedback()
        {
            Kill();
            activeTween = cardTransform
                .DOPunchScale(Vector3.one * 0.12f, 0.3f, 5, 0.5f);
        }
 
        public void Kill() => activeTween?.Kill();
    }
