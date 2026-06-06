/*
 Deck.cs

 Purpose:
 - Represents a shuffled deck of cards and provides draw/discard operations, reshuffling logic and helpers used by the game.

 Commenting guideline applied:
 - File-level header added to follow the project's consistent documentation style.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Represents a shuffled deck of cards and provides draw/discard operations.
    /// </summary>
    public class Deck
    {
        private readonly Random random = new();

        private readonly GameMode _currentGameMode;

        private List<Card> _gameDeck = [];
        private List<Card> _discardPile = [];

        private readonly int _additionalSpecialCards = 2;
        private int _reshuffleCount = 0;

        private bool _deckReshuffled = false;


        public List<Card> DiscardPile
        {
            get { return _discardPile; }
            set { _discardPile = value; }
        }

        public List<Card> GameDeck
        {
            get { return _gameDeck; }
            set { _gameDeck = value; }
        }

        public bool DeckReshuffled
        {
            get { return _deckReshuffled; }
            set { _deckReshuffled = value; }
        }

        public GameMode CurrentGameMode
        {
            get
            {
                return _currentGameMode;
            }
        }

        public int DeckReshuffleCount
        {
            get { return _reshuffleCount; }
            private set { _reshuffleCount = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Deck"/> class and inserts the penalty card into the deck.
        /// </summary>
        public Deck(GameMode currentGameMode)
        {
            _currentGameMode = currentGameMode;

            // -------------- ADDING THE 52 CARDS + SPECIAL CARDS -------------- //

            foreach (Values value in Enum.GetValues<Values>())
                foreach (Suits suit in Enum.GetValues<Suits>())
                    _gameDeck.Add(new RegularCard(suit, value));

            foreach (SpecialCardType specialCard in Enum.GetValues<SpecialCardType>())
            {
                _gameDeck.Add(new SpecialCard(specialCard));
                for (int i = 0; i < _additionalSpecialCards; i++)
                    _gameDeck.Add(new SpecialCard(specialCard));
            }

            // SHUFFLING

            InHouseShuffle();

            // ------------------ PENALTY CARD MANIPULATION ----------------- //

            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            int index = _gameDeck.FindIndex(card => card is RegularCard regularCard && regularCard.IsEqual(penaltyCard));
            _gameDeck.RemoveAt(index);

            int firstPenaltyPositionIndex = 22;
            int secondPenaltyPositionIndex = 45;

            int randomPosition = random.Next(firstPenaltyPositionIndex, secondPenaltyPositionIndex);
            _gameDeck.Insert(randomPosition, penaltyCard);
        }

        /// <summary>
        /// Prevents returning a special card as the initial face-up card by drawing 
        ///     until a non-special card is found.
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
        public void AddRange(List<Card> cardsToAdd) => _gameDeck.AddRange(cardsToAdd);

        /// <summary>
        /// Shuffles the deck in-place using the Fisher-Yates algorithm.
        /// </summary>
        public void InHouseShuffle()
        {
            for (int i = _gameDeck.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);
                (_gameDeck[randomIndex], _gameDeck[i]) = (_gameDeck[i], _gameDeck[randomIndex]);
            }
        }

        /// <summary>
        /// Returns the number of cards remaining in the deck.
        /// </summary>
        /// <returns>The count of cards remaining in the draw pile.</returns>
        public int Length() => _gameDeck.Count;

        /// <summary>
        /// Deals the top card from the deck, reshuffling from the discard pile if necessary.
        /// </summary>
        /// <returns>The dealt card, or null when no cards are available.</returns>
        public Card? DealCard()
        {
            if (_gameDeck.Count > 0)
            {
                Card dealtCard = _gameDeck[0];
                _gameDeck.RemoveAt(0);

                return dealtCard;
            }

            // this will only trigger if the deck is empty AND (game mode is Both OR if the deck has not been reshuffled yet),
            // allowing for one reshuffle in Ascending/Descending modes
            else if (_gameDeck.Count == 0 && (_currentGameMode is GameMode.Both || !_deckReshuffled))
            {
                return ResetDeckAndDealCard();
            }

            else
            {
                return null;
            }
        }

        /// <summary>
        /// Constructs a deck with a premade list of card instances.
        /// </summary>
        /// <param name="preMadeDeck">A list of cards representing a prepared deck.</param>
        public Deck(List<Card> preMadeDeck) => _gameDeck = preMadeDeck;

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

        public void ResetGameDeck()
        {
            Card lastCardOnTable = DiscardPile[DiscardPile.Count - 1];

            DiscardPile.RemoveAt(DiscardPile.Count - 1);

            _gameDeck.AddRange(DiscardPile);
            DiscardPile.Clear();

            InHouseShuffle();
            DiscardPile.Add(lastCardOnTable);

            _deckReshuffled = true;

            _reshuffleCount++;
        }

        public Card? ResetDeckAndDealCard()
        {
            if (DiscardPile.Count == 0 || DiscardPile is null)
                return null;

            ResetGameDeck();

            return DealCard();
        }
    }
}

