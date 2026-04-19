using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Displays the end-of-run overlay (death or victory) and handles restart.
/// Subscribes to GameOverState.OnGameOver and shows the EndGamePanel.
///
/// Wiring: GameStateMachine calls Initialise(gameOverState) after RegisterStates().
/// Sits on the same GameObject as HUDView — shares the UIDocument component.
/// This view never reads GameContext directly.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class GameOverView : MonoBehaviour
{
    // ── Cached element references ─────────────────────────────────────

    private VisualElement endGamePanel;
    private Label resultLabel;
    private Label scoreLabel;
    private Button restartButton;

    // ── Model reference ───────────────────────────────────────────────

    private GameOverState gameOverState;

    // ── Setup ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameStateMachine after states are registered.
    /// </summary>
    public void Initialise(GameOverState state)
    {
        gameOverState = state;

        var root      = GetComponent<UIDocument>().rootVisualElement;
        endGamePanel  = root.Q<VisualElement>("EndGamePanel");
        resultLabel   = root.Q<Label>("GameOverResultLabel");
        scoreLabel    = root.Q<Label>("ScoreLabel");
        restartButton = root.Q<Button>("RestartButton");

        if (endGamePanel == null)
            Debug.LogWarning("[GameOverView] Could not find 'EndGamePanel' in UIDocument.");

        // Panel is hidden by default (USS sets display: none)
        SetPanelVisible(false);

        if (restartButton != null)
            restartButton.clicked += HandleRestart;

        gameOverState.OnGameOver += HandleGameOver;
    }

    private void OnDestroy()
    {
        if (gameOverState != null)
            gameOverState.OnGameOver -= HandleGameOver;

        if (restartButton != null)
            restartButton.clicked -= HandleRestart;
    }

    // ── Event handlers ────────────────────────────────────────────────

    private void HandleGameOver(GameOverResult result, int score)
    {
        if (resultLabel != null)
            resultLabel.text = result == GameOverResult.Victory ? "VICTORY" : "YOU DIED";

        if (scoreLabel != null)
        {
            string prefix = score >= 0 ? "+" : "";
            scoreLabel.text = $"Score: {prefix}{score}";
        }

        SetPanelVisible(true);
    }

    private void HandleRestart()
    {
        SceneManager.LoadScene("Bootstrap");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private void SetPanelVisible(bool visible)
    {
        if (endGamePanel == null) return;
        endGamePanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
