using System.Collections.Generic;
using System.Data.SqlTypes;

namespace SolitaireUno
{
    public class Deck
    {
        private readonly Random random = new();

        private List<Card> _GameDeck = [];
        private List<Card> _DiscardPile = [];

        public List<Card> DiscardPile
        {
            get { return _DiscardPile; }  
            set { _DiscardPile = value; }
        }

        public List<Card> GameDeck
        {
            get { return _GameDeck; }
            set { _GameDeck = value;  }
        }

        private readonly int addtionalSpecialCards = 1;

        private bool _DeckReshuffled = false;

        public bool DeckReshuffled
        {
            get { return _DeckReshuffled; }
            set { _DeckReshuffled = value; }
        }

        public Deck()
        {
            foreach (Values value in Enum.GetValues<Values>())
                foreach (Suits suit in Enum.GetValues<Suits>())
                    _GameDeck.Add(new RegularCard(suit, value));

            foreach (SpecialCardType specialCard in Enum.GetValues<SpecialCardType>())
            {
                _GameDeck.Add(new SpecialCard(specialCard));

                for (int i = 0; i < addtionalSpecialCards; i++)
                    _GameDeck.Add(new SpecialCard(specialCard));
            }

            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            InHouseShuffle();

            int index = _GameDeck.FindIndex(card => card is RegularCard regularCard && regularCard.IsEqual(penaltyCard));
            _GameDeck.RemoveAt(index);

            int firstPenaltyPositionIndex = 22;
            int secondPenaltyPositionIndex = 45;

            int randomPosition = random.Next(firstPenaltyPositionIndex, secondPenaltyPositionIndex);
            _GameDeck.Insert(randomPosition, penaltyCard);
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
        
        public void AddRange(List<Card> cardsToAdd) => _GameDeck.AddRange(cardsToAdd);

        public void InHouseShuffle()
        {
            if (_GameDeck is not null)
            {
                for (int i = _GameDeck.Count - 1; i > 0; i--)
                {
                    int randomIndex = random.Next(0, i + 1);
                    (_GameDeck[randomIndex], _GameDeck[i]) = (_GameDeck[i], _GameDeck[randomIndex]);
                }
            }
        }

        public int Length() => _GameDeck.Count;
        
        public Card? DealCard()
        {
            if (_GameDeck is null)
                return null;

            else
            {
                if (_GameDeck.Count != 0)
                {
                    Card dealtCard = _GameDeck[0];
                    _GameDeck.RemoveAt(0);

                    return dealtCard;
                }

                else
                {
                    if (!_DeckReshuffled)
                    {
                        int lastCardIndex = DiscardPile.Count - 1;
                        Card lastCardOnTable = DiscardPile[lastCardIndex];

                        DiscardPile.RemoveAt(lastCardIndex);

                        _GameDeck.AddRange(DiscardPile);
                        DiscardPile.Clear();

                        InHouseShuffle();
                        DiscardPile.Add(lastCardOnTable);

                        _DeckReshuffled = true;

                        return DealCard();
                    }

                    else
                        return null;
                }
            }
        }

        public Deck(List<Card> preMadeDeck) => _GameDeck = preMadeDeck;
        
        public void AddToDiscardPile(Card card) => DiscardPile.Add(card);

        public static void Empty(List<Card> collectionToBeCleared) => collectionToBeCleared.Clear();
    }
}

