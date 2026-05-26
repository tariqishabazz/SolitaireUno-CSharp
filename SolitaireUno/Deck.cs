using System.Collections.Generic;
using System.Data.SqlTypes;

namespace SolitaireUno
{
    /// <summary>
    /// Represents a shuffled deck of cards and provides draw/discard operations.
    /// </summary>
    public class Deck
    {
        private readonly Random random = new();

        private List<Card> _GameDeck = [];
        private List<Card> _DiscardPile = [];

        private readonly int addtionalSpecialCards = 1;
        private bool _DeckReshuffled = false;

        public List<Card> DiscardPile
        {
            get { return _DiscardPile; }
            set { _DiscardPile = value; }
        }

        public List<Card> GameDeck
        {
            get { return _GameDeck; }
            set { _GameDeck = value; }
        }

        public bool DeckReshuffled
        {
            get { return _DeckReshuffled; }
            set { _DeckReshuffled = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deck"/> class and inserts the penalty card into the deck.
        /// </summary>
        public Deck()
        {
            // -------------- ADDING THE 52 CARDS + SPECIAL CARDS -------------- //

            foreach (Values value in Enum.GetValues<Values>())
                foreach (Suits suit in Enum.GetValues<Suits>())
                    _GameDeck.Add(new RegularCard(suit, value));

            foreach (SpecialCardType specialCard in Enum.GetValues<SpecialCardType>())
            {
                _GameDeck.Add(new SpecialCard(specialCard));

                for (int i = 0; i < addtionalSpecialCards; i++)
                    _GameDeck.Add(new SpecialCard(specialCard));
            }
            
            // SHUFFLING

            InHouseShuffle();

            // ------------------ PENALTY CARD MANIPULATION ----------------- //

            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            int index = _GameDeck.FindIndex(card => card is RegularCard regularCard && regularCard.IsEqual(penaltyCard));
            _GameDeck.RemoveAt(index);

            int firstPenaltyPositionIndex = 22;
            int secondPenaltyPositionIndex = 45;

            int randomPosition = random.Next(firstPenaltyPositionIndex, secondPenaltyPositionIndex);
            _GameDeck.Insert(randomPosition, penaltyCard);
        }

        /// <summary>
        /// Prevents returning a special card as the initial face-up card by drawing until a non-special card is found.
        /// </summary>
        /// <returns>The first non-special card to be used on the table, or null if the deck is empty.</returns>
        public Card? PreventInitialSpecialCard()
        {
            Card firstCard = DealCard()!;

            if (firstCard is null)
                return null;

            while (firstCard is SpecialCard)
            {
                List<Card> temporarySpecialCards = [firstCard];

                if (Length() > 0)
                {
                    firstCard = DealCard()!;
                }

                AddRange(temporarySpecialCards);
                InHouseShuffle();
            }

            return firstCard;
        }

        /// <summary>
        /// Adds a range of cards to the bottom of the game deck.
        /// </summary>
        /// <param name="cardsToAdd">The cards to add to the deck.</param>
        public void AddRange(List<Card> cardsToAdd) => _GameDeck.AddRange(cardsToAdd);

        /// <summary>
        /// Shuffles the deck in-place using the Fisher-Yates algorithm.
        /// </summary>
        public void InHouseShuffle()
        {
            for (int i = _GameDeck.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);
                (_GameDeck[randomIndex], _GameDeck[i]) = (_GameDeck[i], _GameDeck[randomIndex]);
            }
        }

        /// <summary>
        /// Returns the number of cards remaining in the deck.
        /// </summary>
        /// <returns>The count of cards remaining in the draw pile.</returns>
        public int Length() => _GameDeck.Count;

        /// <summary>
        /// Deals the top card from the deck, reshuffling from the discard pile if necessary.
        /// </summary>
        /// <returns>The dealt card, or null when no cards are available.</returns>
        public Card? DealCard()
        {
            if (_GameDeck.Count != 0)
            {
                Card dealtCard = _GameDeck[0];
                _GameDeck.RemoveAt(0);

                return dealtCard;
            }

            if (!_DeckReshuffled)
            {
                Card lastCardOnTable = DiscardPile[DiscardPile.Count - 1];

                DiscardPile.RemoveAt(DiscardPile.Count - 1);

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


        /// <summary>
        /// Constructs a deck with a premade list of card instances.
        /// </summary>
        /// <param name="preMadeDeck">A list of cards representing a prepared deck.</param>
        public Deck(List<Card> preMadeDeck) => _GameDeck = preMadeDeck;

        /// <summary>
        /// Adds a card to the discard pile.
        /// </summary>
        /// <param name="card">The card to add to the discard pile.</param>
        public void AddToDiscardPile(Card card) => DiscardPile.Add(card);

        /// <summary>
        /// Clears the supplied collection.
        /// </summary>
        /// <param name="collectionToBeCleared">The collection to clear.</param>
        public static void Empty(List<Card> collectionToBeCleared) => collectionToBeCleared.Clear();
    }
}

