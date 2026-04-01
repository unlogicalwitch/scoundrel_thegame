using System;
using UnityEngine;

/// <summary>
/// Terminal state — the run is over.
/// Fires OnGameOver with the result so the UI can show the appropriate screen.
/// </summary>
public enum GameOverResult { Death, Victory }
 
public class GameOverState : IGameState
{
    public event Action<GameOverResult> OnGameOver;
 
    public void Enter(GameContext context)
    {
        var result = context.PlayerState.IsDead ? GameOverResult.Death : GameOverResult.Victory;
        Debug.Log($"[GameOverState] Run ended — {result}");
        OnGameOver?.Invoke(result);
    }
 
    public void Exit(GameContext context) { }
}
