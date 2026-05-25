using SolitaireUno;
using System.Numerics;
using System.Collections.Generic;
using System;

namespace SolitaireUno
{
    public class Player
    {
        public List<Card> Hand = [];
        public Player()
        {
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