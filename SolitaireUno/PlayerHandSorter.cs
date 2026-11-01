namespace SolitaireUno
{
    internal class PlayerHandSorter(Player player)
    {
        private readonly Player _player = player;

        /// <summary>
        /// Sorts the player's hand by the numerical value of regular cards while preserving special cards.
        /// </summary>
        public void SortHandByValue()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = _player.Hand.OfType<RegularCard>();

            var sortedValues = (from RegularCard regularCard in allPlayersRegularCards
                                orderby regularCard.Value
                                select regularCard).ToList();

            var sortedSpecials = AllSortedSpecialCards();

            List<Card> sortedHand = [];

            sortedHand.AddRange(sortedValues);
            sortedHand.AddRange(sortedSpecials);

            _player.Hand.Clear();
            _player.Hand.AddRange(sortedHand);
        }

        /// <summary>
        /// Sorts the player's hand by suit order and appends special cards.
        /// </summary>
        public void SortHandBySuit()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = _player.Hand.OfType<RegularCard>();

            var sortedSuits = (from RegularCard regularCard in allPlayersRegularCards
                               orderby regularCard.Suit
                               select regularCard).ToList();

            var sortedSpecials = AllSortedSpecialCards();

            List<Card> sortedHand = [];

            sortedHand.AddRange(sortedSuits);
            sortedHand.AddRange(sortedSpecials);

            _player.Hand.Clear();
            _player.Hand.AddRange(sortedHand);
        }

        /// <summary>
        /// Sorts the player's hand by suit then value, with special cards appended.
        /// </summary>
        public void SortHandBySuitAndValue()
        {
            IEnumerable<RegularCard> allPlayersRegularCards = _player.Hand.OfType<RegularCard>();

            IOrderedEnumerable<RegularCard> sortedSuitsAndValues = allPlayersRegularCards.OrderBy(card => card.Suit)
                                                             .ThenBy(card => card.Value);

            var sortedSpecials = AllSortedSpecialCards();

            List<Card> sortedHand = [];

            sortedHand.AddRange(sortedSuitsAndValues);
            sortedHand.AddRange(sortedSpecials);

            _player.Hand.Clear();
            _player.Hand.AddRange(sortedHand);
        }

        /// <summary>
        /// Returns the player's special cards sorted by their type.
        /// </summary>
        /// <returns>List of special cards sorted by type.</returns>
        private List<SpecialCard> AllSortedSpecialCards()
        {
            IEnumerable<SpecialCard> allPlayersSpecialCards = _player.Hand.OfType<SpecialCard>();

            return (from SpecialCard specialCard in allPlayersSpecialCards
                    orderby specialCard.CardType
                    select specialCard).ToList();
        }
    }
}











































































































































































































































































































































































































































