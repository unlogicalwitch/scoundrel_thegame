using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MonoBehaviour bridge that loads all CardSO assets from a Resources folder
/// and hands them to DeckManager. Attach to a Bootstrap GameObject in your
/// Bootstrap scene. Swap this loader out later for an Addressables version
/// without touching any game logic.
/// </summary>
public class DeckLoader : MonoBehaviour
{
    [Tooltip("Path inside a Resources folder where all CardSO assets live.")]
    [SerializeField] private string cardResourcesPath = "Data/Cards";
 
    /// <summary>
    /// Load all CardSO assets and initialise the provided DeckManager.
    /// Returns the number of cards loaded into the deck.
    /// </summary>
    public int LoadAndInitialise(DeckManager deckManager)
    {
        var cards = Resources.LoadAll<CardSO>(cardResourcesPath);
 
        if (cards == null || cards.Length == 0)
        {
            Debug.LogError($"[DeckLoader] No CardSO assets found at Resources/{cardResourcesPath}. " +
                           "Create your CardSO assets and place them there.");
            return 0;
        }
 
        deckManager.Initialise(cards);
 
        Debug.Log($"[DeckLoader] Loaded {cards.Length} card assets → " +
                  $"{deckManager.DrawPileCount} cards in Scoundrel deck.");
 
        return deckManager.DrawPileCount;
    }
}
