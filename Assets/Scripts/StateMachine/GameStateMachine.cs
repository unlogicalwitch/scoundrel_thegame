    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives the Scoundrel turn loop. Owns all states and the shared context.
/// Attach to a persistent GameObject in the Game scene.
/// </summary>
public class GameStateMachine : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [SerializeField] private int startingHealth = 20;
    [SerializeField] private RoomView roomView;
    [SerializeField] private InputHandler inputHandler;

    // ── State ────────────────────────────────────────────────────────

    private GameContext context;
    private IGameState  currentState;
    private readonly Dictionary<Type, IGameState> states = new();

    // ── Events ───────────────────────────────────────────────────────

    public event Action<IGameState> OnStateChanged;

    // ── Public API ───────────────────────────────────────────────────

    public GameContext Context => context;
    public IGameState  CurrentState => currentState;

    // ── Lifecycle ────────────────────────────────────────────────────

    private IEnumerator Start()
    {
        var deckManager = ServiceLocator.Get<DeckManager>();
        var playerState = new PlayerState(startingHealth);
        var dungeonRoom = new DungeonRoom();
        context = new GameContext(deckManager, playerState, dungeonRoom, this);
        
        RegisterStates();
        
        yield return new WaitForSeconds(2f);
        var playerChoiceState = (PlayerChoiceState)states[typeof(PlayerChoiceState)];
        roomView.Initialise(context.DungeonRoom, playerChoiceState);
        inputHandler?.Initialise(playerChoiceState);
        
        TransitionTo<DrawingState>();
    }

    // ── Internal ─────────────────────────────────────────────────────

    private void RegisterStates()
    {
        states[typeof(DrawingState)] = new DrawingState();
        states[typeof(PlayerChoiceState)] = new PlayerChoiceState();
        states[typeof(ResolvingState)] = new ResolvingState();
        states[typeof(FleeState)] = new FleeState();
        states[typeof(GameOverState)] = new GameOverState();
    }

    public void TransitionTo<T>() where T : IGameState
    {
        if (!states.TryGetValue(typeof(T), out var next))
        {
            Debug.LogError($"[GameStateMachine] State {typeof(T).Name} is not registered.");
            return;
        }

        currentState?.Exit(context);
        currentState = next;
        currentState.Enter(context);
        OnStateChanged?.Invoke(currentState);

        Debug.Log($"[GameStateMachine] → {typeof(T).Name}");
    }
}
