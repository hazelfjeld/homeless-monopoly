using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardDeck
{
    public string DeckName;
    public List<Card> StartingCards = new List<Card>();

    [SerializeField]
    private List<Card> drawPile = new List<Card>();

    [SerializeField]
    private List<Card> discardPile = new List<Card>();

    public void ResetDeck()
    {
        drawPile = new List<Card>(StartingCards);
        discardPile.Clear();
        ShuffleDrawPile();
    }

    public Card Draw()
    {
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardsIntoDrawPile();
        }

        if (drawPile.Count == 0)
        {
            Debug.LogWarning($"Deck '{DeckName}' is empty.");
            return null;
        }

        Card drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        discardPile.Add(drawnCard);

        return drawnCard;
    }

    public void ShuffleDrawPile()
    {
        for (int currentIndex = 0; currentIndex < drawPile.Count; currentIndex++)
        {
            int swapIndex = UnityEngine.Random.Range(currentIndex, drawPile.Count);

            Card tempCard = drawPile[currentIndex];
            drawPile[currentIndex] = drawPile[swapIndex];
            drawPile[swapIndex] = tempCard;
        }
    }

    private void ReshuffleDiscardsIntoDrawPile()
    {
        drawPile = new List<Card>(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
    }
}