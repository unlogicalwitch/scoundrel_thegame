using UnityEngine;

public interface IGameState
{
    void Enter(GameContext context);
    void Exit(GameContext context);
}

