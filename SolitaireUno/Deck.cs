using System.Collections.Generic;
using System.Data.SqlTypes;

namespace SolitaireUno
{
    public class Deck
    {
        private readonly Random random = new();
     
        private readonly List<Card> gameDeck = [];
        private readonly List<Card> discardPile = [];
        
        private readonly int addtionalSpecialCards = 1;

        public bool deckReshuffled = false;

        public Deck()
        {
            foreach (Values value in Enum.GetValues<Values>())
            {
                foreach (Suits suit in Enum.GetValues<Suits>())
                {
                    gameDeck.Add(new RegularCard(suit, value));
                }
            }

            foreach (SpecialCardType specialCard in Enum.GetValues<SpecialCardType>())
            {
                gameDeck.Add(new SpecialCard(specialCard));

               
                //if (specialCard != SpecialCardType.ChangeOrder)
                //{
                    for (int i = 0; i < addtionalSpecialCards; i++)
                    {
                        gameDeck.Add(new SpecialCard(specialCard));
                    }
                //}
            }

            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            InHouseShuffle();

            int index = 0;
            foreach (Card card in gameDeck)
            {
                if (card is RegularCard regularCard)
                    if (regularCard.IsEqual(penaltyCard))
                        break;
                index++;
            }

            gameDeck.RemoveAt(index);
            int randomPosition = random.Next(22, 45);
            gameDeck.Insert(randomPosition, penaltyCard);

        }

        public void AddRange(List<Card> cardsToAdd)
        {
            gameDeck.AddRange(cardsToAdd);
        }

        public void InHouseShuffle()
        {
            if (gameDeck is not null)
            {
                for (int i = gameDeck.Count - 1; i > 0; i--)
                {
                    int randomIndex = random.Next(0, i + 1);
                    (gameDeck[randomIndex], gameDeck[i]) = (gameDeck[i], gameDeck[randomIndex]);
                }
            }
        }

        public int Length()
        {
            return gameDeck.Count;
        }
        
        public Card? DealCard()
        {
            switch (gameDeck)
            {
                case not null:
                    {
                        if (gameDeck.Count != 0)
                        {
                            Card dealtCard = gameDeck[0];
                            gameDeck.RemoveAt(0);

                            return dealtCard;
                        }
                        else
                        {
                            if (!deckReshuffled)
                            {
                                int lastCardIndex = discardPile.Count - 1;
                                Card lastCardOnTable = discardPile[lastCardIndex];

                                discardPile.RemoveAt(lastCardIndex);
                               // discardPile.RemoveAll(card => card is SpecialCard specialCard && specialCard.CardType.Equals(SpecialCardType.ChangeOrder));

                                gameDeck.AddRange(discardPile);
                                discardPile.Clear();

                                InHouseShuffle();
                                discardPile.Add(lastCardOnTable);

                                deckReshuffled = true;

                                return DealCard();
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                default:
                    return null;
            }
        }
        
        public Deck(List<Card> preMadeDeck)
        {
            gameDeck = preMadeDeck;
        }
        
        public void AddToDiscardPile(Card card)
        {
            discardPile.Add(card);
        }
    }
}

