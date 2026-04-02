using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Entry point for the entire application.
/// Runs once in the Bootstrap scene before any gameplay scene loads.
///
/// Responsibilities (in order):
///   1. Instantiate all pure-C# services (DeckManager, etc.)
///   2. Load card assets via DeckLoader and initialise DeckManager
///   3. Register services in ServiceLocator
///   4. Load the Gameplay scene additively (or directly)
///
/// Add new service registrations here — nowhere else.
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Exact name of the gameplay scene to load after bootstrap.")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Services")]
    [SerializeField] private DeckLoader deckLoader;

    private void Awake()
    {
        // ── 1. Create services ───────────────────────────────────────
        var deckManager = new DeckManager();
        
        // ── 2. Register services ─────────────────────────────────────
        ServiceLocator.Register(deckManager);

        // ── 3. Load and initialise the deck ─────────────────────────
        int cardCount = deckLoader.LoadAndInitialise(deckManager);

        if (cardCount == 0)
        {
            Debug.LogError("[Bootstrap] Deck initialisation failed — no cards loaded. " +
                           "Ensure CardSO assets exist under Resources/Data/Cards/.");
            return; // Do not proceed to gameplay with an empty deck
        }

        // ── 4. Load gameplay scene ───────────────────────────────────
        Debug.Log($"[Bootstrap] Services ready. Loading scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
}