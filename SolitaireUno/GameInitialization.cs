namespace SolitaireUno
{
    internal class GameInitialization(Deck deck)
    {
        private readonly Deck _deck = deck;

        /// <summary>
        /// Prevents returning a special card as the initial face-up card by drawing
        ///     until a non-special card is found.
        /// </summary>
        /// <returns>The first non-special card to be used on the table, or null if the deck is empty.</returns>
        public Card? PreventInitialSpecialCard()
        {
            Card? firstCard = _deck.DealCard();

            if (firstCard is null)
                return null;

            while (firstCard is SpecialCard)
            {
                List<Card> temporarySpecialCards = [ firstCard ];

                if (_deck.Length() > 0)
                    firstCard = _deck.DealCard();

                _deck.AddRange(temporarySpecialCards);
                _deck.InHouseShuffle();
            }

            return firstCard;
        }
    }
}
