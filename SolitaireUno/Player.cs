/*
 Player.cs

 Purpose:
 - Represents a game participant that holds a hand of cards and can play or pick up cards.
 - Contains sorting helpers for the player's hand and utility methods for pickup/play.

 Commenting guideline applied:
 - File-level purpose header added to follow Home.razor.cs style. Inline method summaries are preserved.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Represents a game participant that holds a hand of cards and can play or pick up cards.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of <see cref="Player"/> and deals an initial hand from the game deck.
    /// </remarks>
    /// <param name="gameDeck">The deck used to draw the initial hand.</param>
    public class Player()
    {
        public List<Card> Hand = [];
        public required string Name { get; init; }

        /// <summary>
        /// Adds a card to the player's hand.
        /// </summary>
        /// <param name="card">The card to pick up.</param>
        public void PickupCard(Card card)
        {
            Hand.Add(card);
        }

        /// <summary>
        /// Removes a card from the player's hand, representing play.
        /// </summary>
        /// <param name="card">The card to play.</param>
        public void PlayCard(Card card)
        {
            Hand.Remove(card);
        }

        /// <summary>
        /// Sorts the player's hand by the numerical value of regular cards while preserving special cards.
        /// </summary>
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

        /// <summary>
        /// Sorts the player's hand by suit order and appends special cards.
        /// </summary>
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

        /// <summary>
        /// Sorts the player's hand by suit then value, with special cards appended.
        /// </summary>
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

        /// <summary>
        /// Returns the player's special cards sorted by their type.
        /// </summary>
        /// <returns>List of special cards sorted by type.</returns>
        private List<SpecialCard> AllSortedSpecialCards()
        {
            IEnumerable<SpecialCard> allPlayersSpecialCards = Hand.OfType<SpecialCard>();

            return (from SpecialCard specialCard in allPlayersSpecialCards
                    orderby specialCard.CardType
                    select specialCard).ToList();
        }
    }
}