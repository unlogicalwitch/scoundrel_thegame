using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Entry point for the entire application.
/// Runs once in the Bootstrap scene before any gameplay scene loads.
///
/// Responsibilities (in order):
///   1. Instantiate all pure-C# services (DeckManager, etc.)
///   2. Load card assets via DeckLoader and initialise DeckManager
///   3. Register services in ServiceLocator
///   4. Reveal the Start button — scene loads only when the player presses it
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

    [Header("UI")]
    [SerializeField] private Button startButton;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        // Hide the button until services are confirmed ready
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        var deckManager = new DeckManager();
        ServiceLocator.Register(deckManager);

        // Load deck assets
        int cardCount = deckLoader.LoadAndInitialise(deckManager);

        if (cardCount == 0)
        {
            Debug.LogError("[Bootstrap] Deck initialisation failed — no cards loaded. " +
                           "Ensure CardSO assets exist under Resources/Data/Cards/.");
            return; // Do not proceed to gameplay with an empty deck
        }

        Debug.Log($"[Bootstrap] Services ready ({cardCount} cards loaded). Awaiting player input.");
        
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(OnStartPressed);
        }
        else
        {
            Debug.LogWarning("[Bootstrap] No start button assigned — loading scene immediately.");
            LoadGameplay();
        }
    }

    private void OnStartPressed()
    {
        startButton.interactable = false;
        LoadGameplay();
    }

    private void LoadGameplay()
    {
        Debug.Log($"[Bootstrap] Loading scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
}
