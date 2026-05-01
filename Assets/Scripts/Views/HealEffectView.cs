using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the full-screen heal vignette effect on the Canvas/Image.
/// Subscribes to PlayerState.OnHealthChanged and pulses the shader's
/// _Intensity property via DOTween whenever a heal occurs (positive delta).
///
/// Wiring:
///   GameStateMachine calls Initialise(playerState) after building GameContext.
///   The Image component on this GameObject must use Shader Graphs_HealthEffectShader.mat.
/// </summary>
[RequireComponent(typeof(Image))]
public class HealEffectView : MonoBehaviour
{
    // ── Config ───────────────────────────────────────────────────────

    private static readonly float PeakIntensity  = 0.5f;
    private static readonly float FadeInDuration  = 0.15f;
    private static readonly float HoldDuration    = 0.25f;
    private static readonly float FadeOutDuration = 0.35f;
    private static readonly Ease  FadeInEase      = Ease.InCubic;
    private static readonly Ease  FadeOutEase     = Ease.InCubic;

    private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");

    // ── References ───────────────────────────────────────────────────

    private Image image;
    private Material instanceMaterial;
    private PlayerState playerState;
    private Tween activeTween;

    // ── Lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        image = GetComponent<Image>();

        // Use an instance material so we don't mutate the shared asset.
        instanceMaterial = Instantiate(image.material);
        image.material = instanceMaterial;

        // Start fully invisible.
        instanceMaterial.SetFloat(IntensityProperty, 0f);
    }

    /// <summary>
    /// Called by GameStateMachine after GameContext is built.
    /// </summary>
    public void Initialise(PlayerState player)
    {
        playerState = player;
        playerState.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        activeTween?.Kill();

        if (playerState != null)
            playerState.OnHealthChanged -= HandleHealthChanged;

        // Clean up the instance material.
        if (instanceMaterial != null)
            Destroy(instanceMaterial);
    }

    // ── Event handler ─────────────────────────────────────────────────

    private void HandleHealthChanged(int currentHealth, int changeAmount)
    {
        // Only react to heals (positive delta).
        if (changeAmount <= 0) return;

        PlayHealPulse();
    }

    // ── Animation ────────────────────────────────────────────────────

    private void PlayHealPulse()
    {
        activeTween?.Kill();

        // Reset to 0 before starting so overlapping heals restart cleanly.
        instanceMaterial.SetFloat(IntensityProperty, 0f);

        activeTween = DOTween.Sequence()
            .Append(DOTween.To(
                () => instanceMaterial.GetFloat(IntensityProperty),
                v  => instanceMaterial.SetFloat(IntensityProperty, v),
                PeakIntensity,
                FadeInDuration).SetEase(FadeInEase))
            .AppendInterval(HoldDuration)
            .Append(DOTween.To(
                () => instanceMaterial.GetFloat(IntensityProperty),
                v  => instanceMaterial.SetFloat(IntensityProperty, v),
                0f,
                FadeOutDuration).SetEase(FadeOutEase))
            .Play();
    }
}
