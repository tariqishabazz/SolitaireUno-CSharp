using SolitaireUno;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

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

        public void SortHandByValue()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = Hand.OfType<RegularCard>();

            var sortedValues = (from RegularCard regularCard in allPlayersRegularCards
                                orderby regularCard.Value
                                select regularCard).ToList();

            var sortedSpecials = AllSortedSpecialCards();

            List<Card> sortedHand = [];
            
            sortedHand.AddRange(sortedValues);
            sortedHand.AddRange(sortedSpecials);

            Hand = sortedHand;
        }

        public void SortHandBySuit()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = Hand.OfType<RegularCard>();

            var sortedSuits = (from RegularCard regularCard in allPlayersRegularCards
                                orderby regularCard.Suit
                                select regularCard).ToList();


            var sortedSpecials = AllSortedSpecialCards();

            List<Card> sortedHand = [];
            
            sortedHand.AddRange(sortedSuits);
            sortedHand.AddRange(sortedSpecials);

            Hand = sortedHand;
        }

        public void SortHandBySuitAndValue()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = Hand.OfType<RegularCard>();

            var sortedSuitsAndValues = (from RegularCard regularCard in allPlayersRegularCards
                                        orderby regularCard.Value, regularCard.Suit
                                        select regularCard).ToList();


            var sortedSpecials = AllSortedSpecialCards();


            List<Card> sortedHand = [];

            sortedHand.AddRange(sortedSuitsAndValues);
            sortedHand.AddRange(sortedSpecials);

            Hand = sortedHand;
        }

        private List<SpecialCard> AllSortedSpecialCards()
        {
            IEnumerable<SpecialCard> allPlayersSpecialCards = Hand.OfType<SpecialCard>();

            return (from SpecialCard specialCard in allPlayersSpecialCards
                    orderby specialCard.CardType
                    select specialCard).ToList();
        }
    }
}