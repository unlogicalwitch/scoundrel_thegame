using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// View layer for the settings panel.
/// Subscribes to GameSettings events and updates UI accordingly.
/// Calls GameSettings when user interacts with toggles.
/// </summary>
public class SettingsView : MonoBehaviour
{
    // ── Dependencies ─────────────────────────────────────────────────
    
    private GameSettings gameSettings;
    private UIDocument uiDocument;
    
    // ── UI Elements ──────────────────────────────────────────────────
    
    private Toggle consecutiveFleeToggle;
    
    // ── Lifecycle ────────────────────────────────────────────────────
    
    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[SettingsView] No UIDocument found on GameObject");
            return;
        }
    }
    
    private void Start()
    {
        // Get GameSettings from ServiceLocator
        gameSettings = ServiceLocator.Get<GameSettings>();
        if (gameSettings == null)
        {
            Debug.LogError("[SettingsView] GameSettings not found in ServiceLocator");
            return;
        }
        
        // Get UI elements
        var root = uiDocument.rootVisualElement;
        consecutiveFleeToggle = root.Q<Toggle>("ConsecutiveFleeToggle");
        
        if (consecutiveFleeToggle == null)
        {
            Debug.LogError("[SettingsView] ConsecutiveFleeToggle not found in UI");
            return;
        }
        
        // Initialize toggle state from settings
        consecutiveFleeToggle.value = gameSettings.ConsecutiveFleeAllowed;
        
        // Subscribe to UI events
        consecutiveFleeToggle.RegisterValueChangedCallback(OnConsecutiveFleeToggleChanged);
        
        // Subscribe to settings events
        gameSettings.OnConsecutiveFleeChanged += OnConsecutiveFleeSettingChanged;
        
        Debug.Log("[SettingsView] Initialized");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from UI events
        if (consecutiveFleeToggle != null)
        {
            consecutiveFleeToggle.UnregisterValueChangedCallback(OnConsecutiveFleeToggleChanged);
        }
        
        // Unsubscribe from settings events
        if (gameSettings != null)
        {
            gameSettings.OnConsecutiveFleeChanged -= OnConsecutiveFleeSettingChanged;
        }
    }
    
    // ── Event Handlers ───────────────────────────────────────────────
    
    /// <summary>
    /// Called when the user toggles the consecutive flee toggle in the UI.
    /// Updates the GameSettings.
    /// </summary>
    private void OnConsecutiveFleeToggleChanged(ChangeEvent<bool> evt)
    {
        if (gameSettings != null)
        {
            gameSettings.ConsecutiveFleeAllowed = evt.newValue;
        }
    }
    
    /// <summary>
    /// Called when the GameSettings consecutive flee setting changes.
    /// Updates the UI toggle to match (in case it was changed elsewhere).
    /// </summary>
    private void OnConsecutiveFleeSettingChanged(bool newValue)
    {
        if (consecutiveFleeToggle != null && consecutiveFleeToggle.value != newValue)
        {
            consecutiveFleeToggle.value = newValue;
        }
    }
}
