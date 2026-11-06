namespace SolitaireUno
{
    public class Deck
    {
        private readonly Random _random = Random.Shared;
        private readonly GameMode _currentGameMode;

        private readonly List<Card> _gameDeck = [];
        private readonly Stack<Card> _discardPile = [];

        private readonly int _additionalSpecialCards = 1;
        private int _reshuffleCount;

        public int ReshuffleCount
        {
            get => _reshuffleCount;
            set => _reshuffleCount = value;
        }

        public Stack<Card> DiscardPile => _discardPile;
        public List<Card> GameDeck => _gameDeck;
        public GameMode CurrentGameMode => _currentGameMode;

        public Deck(GameMode currentGameMode)
        {
            _currentGameMode = currentGameMode;
            InitializeStandardDeck();
            InHouseShuffle();
        }

        public Deck(List<Card> preMadeDeck, GameMode currentGameMode = GameMode.Ascending)
        {
            _currentGameMode = currentGameMode;
            _gameDeck.AddRange(preMadeDeck);
        }

        private void InitializeStandardDeck()
        {
            foreach (Values value in Enum.GetValues<Values>())
            {
                foreach (Suits suit in Enum.GetValues<Suits>())
                {
                    _gameDeck.Add(new RegularCard(suit, value));
                }
            }

            foreach (TypeOfSpecialCard specialCard in Enum.GetValues<TypeOfSpecialCard>())
            {
                _gameDeck.Add(new SpecialCard(specialCard));
                for (int i = 0; i < _additionalSpecialCards; i++)
                {
                    _gameDeck.Add(new SpecialCard(specialCard));
                }
            }
        }

        public void AddRange(IEnumerable<Card> cardsToAdd) => _gameDeck.AddRange(cardsToAdd);

        public void InHouseShuffle()
        {
            for (int i = _gameDeck.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);
                (_gameDeck[ randomIndex ], _gameDeck[ i ]) = (_gameDeck[ i ], _gameDeck[ randomIndex ]);
            }
        }

        public int Length() => _gameDeck.Count;

        public Card? DealCard()
        {
            int maxAllowedReshuffles = _currentGameMode == GameMode.Both ? 2 : 1;

            if (_gameDeck.Count > 0)
            {
                Card dealt = _gameDeck[ 0 ];
                _gameDeck.RemoveAt(0);
                return dealt;
            }

            return _reshuffleCount < maxAllowedReshuffles ? ResetDeckAndDealCard() : null;
        }

        public void AddToDiscardPile(Card card) => _discardPile.Push(card);

        public Card? ReverseDiscard()
        {
            if (_discardPile.Count > 1 && _discardPile.TryPop(out Card? pulledCard))
                return pulledCard;

            return null;
        }

        public void ResetGameDeck()
        {
            if (_discardPile.Count == 0)
                return;

            Card last = _discardPile.Pop();

            _gameDeck.AddRange(_discardPile);
            _discardPile.Clear();

            InHouseShuffle();

            _discardPile.Push(last);
            _reshuffleCount++;
        }

        public Card? ResetDeckAndDealCard()
        {
            if (_discardPile.Count == 0)
                return null;

            ResetGameDeck();
            return DealCard();
        }
    }
}