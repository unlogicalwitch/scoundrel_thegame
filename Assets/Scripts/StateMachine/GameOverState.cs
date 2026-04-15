using System;
using UnityEngine;

/// <summary>
/// Terminal state — the run is over.
/// Callers must call SetResult(result, score) before TransitionTo&lt;GameOverState&gt;().
/// Fires OnGameOver(result, score) so the view layer can display the end screen.
/// </summary>
public enum GameOverResult { Death, Victory }

public class GameOverState : IGameState
{
    // ── Pre-set data (via SetResult before TransitionTo) ─────────────

    private GameOverResult pendingResult;
    private int pendingScore;
    private bool resultSet;

    // ── Events ───────────────────────────────────────────────────────

    public event Action<GameOverResult, int> OnGameOver;

    // ── SetX API ─────────────────────────────────────────────────────

    /// <summary>
    /// Call this before TransitionTo&lt;GameOverState&gt;() to supply the result and score.
    /// </summary>
    public void SetResult(GameOverResult result, int score)
    {
        pendingResult = result;
        pendingScore  = score;
        resultSet     = true;
    }

    // ── IGameState ───────────────────────────────────────────────────

    public void Enter(GameContext context)
    {
        if (!resultSet)
        {
            // Fallback: infer result from player state (no score available)
            pendingResult = context.PlayerState.IsDead ? GameOverResult.Death : GameOverResult.Victory;
            pendingScore  = 0;
            Debug.LogWarning("[GameOverState] SetResult() was not called before Enter — score will be 0.");
        }

        Debug.Log($"[GameOverState] Run ended — {pendingResult}, score: {pendingScore}");
        OnGameOver?.Invoke(pendingResult, pendingScore);

        // Reset for potential future use (e.g. restart without re-registering states)
        resultSet = false;
    }

    public void Exit(GameContext context) { }
}
