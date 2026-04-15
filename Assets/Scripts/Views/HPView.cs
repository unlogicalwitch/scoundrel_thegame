using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class HPView : MonoBehaviour
{
    private TextMeshPro tmp;
    private PlayerState playerState;

    private void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void Initialise(PlayerState playerState)
    {
        this.playerState = playerState;
        
        UpdateDisplay(playerState.CurrentHealth, playerState.MaxHealth);
        playerState.OnHealthChanged += UpdateDisplay;
    }

    private void UpdateDisplay(int current,  int maxHealth)
    {
        tmp.text = current.ToString();
    }

    private void OnDestroy() // unsubscribe cleanly
    {
        playerState.OnHealthChanged -= UpdateDisplay;
    }
}
