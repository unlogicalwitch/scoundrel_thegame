using UnityEngine;

/// <summary>
/// Shared data container passed to every game state.
/// States read and mutate context rather than holding their own references.
/// </summary>
public class GameContext
{
    public DeckManager  DeckManager  { get; }
    public PlayerState  PlayerState  { get; }
    public DungeonRoom  DungeonRoom  { get; }
 
    public GameContext(DeckManager deckManager, PlayerState playerState, DungeonRoom dungeonRoom)
    {
        DeckManager = deckManager;
        PlayerState = playerState;
        DungeonRoom = dungeonRoom;
    }
}
