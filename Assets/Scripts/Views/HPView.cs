using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshPro))]
public class HPView : MonoBehaviour
{
    [SerializeField] private GameObject tmpPopup;
    
    private TextMeshPro tmp;
    private PlayerState playerState;

    private void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    public void Initialise(PlayerState playerState)
    {
        this.playerState = playerState;
        
        UpdateDisplay(playerState.CurrentHealth);
        playerState.OnHealthChanged += HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentAmount, int changeAmount)
    {
        UpdateDisplay(currentAmount);

        var position = transform.position;
        var popup = Instantiate(tmpPopup, new Vector2(position.x, position.y + 0.5f), Quaternion.identity);
        var text = popup.GetComponentInChildren<TextMeshPro>();
        text.color = Color.clear;
        text.text = changeAmount.ToString("+#;-#;0");

        DOTween.Sequence()
            .Append(text.DOColor(Color.white, 0.25f))
            .Join(popup.transform.DOMove(new Vector2(position.x, position.y + 0.75f), 0.75f))
            .OnComplete(() => Destroy(popup.gameObject))
            .Play();
    }

    private void UpdateDisplay(int current)
    {
        tmp.text = current.ToString();
    }

    private void OnDestroy() // unsubscribe cleanly
    {
        playerState.OnHealthChanged -= HandleHealthChanged;
    }
}
