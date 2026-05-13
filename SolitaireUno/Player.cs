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
            // sets up hands with 10 cards each
            for (int i = 0; i < 10; i++)
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