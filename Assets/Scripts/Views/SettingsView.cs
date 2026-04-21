using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// View layer for the settings panel.
/// Reads from and writes to GameSettings when user interacts with toggles.
/// </summary>
public class SettingsView : MonoBehaviour
{
    // ── Dependencies ─────────────────────────────────────────────────
    
    private GameSettings gameSettings;
    private UIDocument uiDocument;
    
    // ── UI Elements ──────────────────────────────────────────────────
    
    private Toggle consecutiveFleeToggle;
    private Toggle consumeMultiplePotionToggle;
    private Button settingsPanelCloseButton;
    private VisualElement settingsPanel;
    
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
        consumeMultiplePotionToggle = root.Q<Toggle>("ConsumeMultiplePotionToggle");
        settingsPanelCloseButton = root.Q<Button>("SettingsPanelCloseButton");
        settingsPanel = root.Q<VisualElement>("SettingsPanel");
        
        if (consecutiveFleeToggle == null)
        {
            Debug.LogError("[SettingsView] ConsecutiveFleeToggle not found in UI");
        }
        
        if (consumeMultiplePotionToggle == null)
        {
            Debug.LogError("[SettingsView] ConsumeMultiplePotionToggle not found in UI");
        }
        
        if (settingsPanelCloseButton == null)
        {
            Debug.LogWarning("[SettingsView] SettingsPanelCloseButton not found in UI");
        }
        
        if (settingsPanel == null)
        {
            Debug.LogWarning("[SettingsView] SettingsPanel not found in UI");
        }
        
        // Initialize toggle states from settings
        if (consecutiveFleeToggle != null)
        {
            consecutiveFleeToggle.value = gameSettings.ConsecutiveFleeAllowed;
            consecutiveFleeToggle.RegisterValueChangedCallback(OnConsecutiveFleeToggleChanged);
        }
        
        if (consumeMultiplePotionToggle != null)
        {
            consumeMultiplePotionToggle.value = gameSettings.ConsumeMultiplePotionAllowed;
            consumeMultiplePotionToggle.RegisterValueChangedCallback(OnConsumeMultiplePotionToggleChanged);
        }
        
        if (settingsPanelCloseButton != null)
        {
            settingsPanelCloseButton.clicked += CloseSettingsPanel;
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.None;
        }
        
        Debug.Log("[SettingsView] Initialized");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from UI events
        if (consecutiveFleeToggle != null)
        {
            consecutiveFleeToggle.UnregisterValueChangedCallback(OnConsecutiveFleeToggleChanged);
        }
        
        if (consumeMultiplePotionToggle != null)
        {
            consumeMultiplePotionToggle.UnregisterValueChangedCallback(OnConsumeMultiplePotionToggleChanged);
        }
        
        if (settingsPanelCloseButton != null)
        {
            settingsPanelCloseButton.clicked -= CloseSettingsPanel;
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
    /// Called when the user toggles the consume multiple potion toggle in the UI.
    /// Updates the GameSettings.
    /// </summary>
    private void OnConsumeMultiplePotionToggleChanged(ChangeEvent<bool> evt)
    {
        if (gameSettings != null)
        {
            gameSettings.ConsumeMultiplePotionAllowed = evt.newValue;
        }
    }
    
    /// <summary>
    /// Closes the settings panel.
    /// </summary>
    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.None;
        }
    }
}
