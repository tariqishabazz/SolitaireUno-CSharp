namespace SolitaireUno
{
    internal static class DeckFactory
    {
        private const int FirstPenaltyPositionIndex = 30;
        private const int SecondPenaltyPositionIndex = 50;

        public static Deck Create(GameMode currentGameMode)
        {
            var deck = new Deck(currentGameMode);
            InsertPenaltyCards(deck.GameDeck);
            return deck;
        }

        public static void InsertPenaltyCards(List<Card> gameDeck)
        {
            List<RegularCard> penaltyCards = [ new(Suits.Spades, Values.Queen), new(Suits.Spades, Values.Ace) ];

            gameDeck.RemoveAll(card => card is RegularCard regularCard && penaltyCards.Any(penaltyCard => regularCard.IsEqual(penaltyCard)));

            foreach (RegularCard penaltyCard in penaltyCards)
            {
                int randomPosition = Random.Shared.Next(FirstPenaltyPositionIndex, SecondPenaltyPositionIndex);
                gameDeck.Insert(randomPosition, penaltyCard);
            }
        }
    }
}
