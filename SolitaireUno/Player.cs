using SolitaireUno;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace SolitaireUno
{
    public class Player
    {
        public List<Card> Hand = [];

        public Player(Deck gameDeck)
        {
            int initialHandSize = 10;
            
            // sets up hands with cards
            for (int i = 0; i < initialHandSize; i++)
            {
                Card playerCard = gameDeck.DealCard()!;
                PickupCard(playerCard);
            }
        }
        public void PickupCard(Card card)
        {
            Hand.Add(card);
        }
        public void PlayCard(Card card)
        {
            Hand.Remove(card);
        }
    }
}