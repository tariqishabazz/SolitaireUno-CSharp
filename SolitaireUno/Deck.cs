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
                foreach (Suits suit in Enum.GetValues<Suits>())
                    gameDeck.Add(new RegularCard(suit, value));

            foreach (SpecialCardType specialCard in Enum.GetValues<SpecialCardType>())
            {
                gameDeck.Add(new SpecialCard(specialCard));

                for (int i = 0; i < addtionalSpecialCards; i++)
                    gameDeck.Add(new SpecialCard(specialCard));
            }

            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            InHouseShuffle();

            int index = gameDeck.FindIndex(card => card is RegularCard regularCard && regularCard.IsEqual(penaltyCard));
            gameDeck.RemoveAt(index);

            int firstPenaltyPositionIndex = 22;
            int secondPenaltyPositionIndex = 45;

            int randomPosition = random.Next(firstPenaltyPositionIndex, secondPenaltyPositionIndex);
            gameDeck.Insert(randomPosition, penaltyCard);

            PreventInitialSpecialCard();
        }

        public Card? PreventInitialSpecialCard()
        {
            Card firstCard = DealCard()!; 
            
            if (firstCard is null)
                return null;

            while (firstCard is SpecialCard)
            {
                List<Card> temporarySpecialCards = [firstCard];

                if (Length() > 0)
                    firstCard = DealCard()!;

                AddRange(temporarySpecialCards);
                InHouseShuffle();
            }

            return firstCard;
        }
        
        public void AddRange(List<Card> cardsToAdd) => gameDeck.AddRange(cardsToAdd);

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

        public int Length() => gameDeck.Count;
        
        public Card? DealCard()
        {
            if (gameDeck is null)
                return null;

            else
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

                        gameDeck.AddRange(discardPile);
                        discardPile.Clear();

                        InHouseShuffle();
                        discardPile.Add(lastCardOnTable);

                        deckReshuffled = true;

                        return DealCard();
                    }

                    else
                        return null;
                }
            }
        }

        public Deck(List<Card> preMadeDeck) => gameDeck = preMadeDeck;
        
        public void AddToDiscardPile(Card card) => discardPile.Add(card);
        
    }
}

