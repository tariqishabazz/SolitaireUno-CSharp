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
    public class Player()
    {
        readonly private List<Card> _hand = [];
        public List<Card> Hand { get => _hand; }

        public required string Name { get; init; }

        /// <summary>
        /// Adds a card to the player's hand.
        /// </summary>
        /// <param name="card">The card to pick up.</param>
        public void PickupCard(Card card)
        {
            _hand.Add(card);
        }

        /// <summary>
        /// Removes a card from the player's hand, representing play.
        /// </summary>
        /// <param name="card">The card to play.</param>
        public void PlayCard(Card card)
        {
            _hand.Remove(card);
        }
    }
}