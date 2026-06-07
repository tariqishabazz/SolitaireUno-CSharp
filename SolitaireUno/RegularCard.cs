// RegularCard.cs — concrete card with suit and value, plus ToString and equality helper.

namespace SolitaireUno
{
    /// <summary>
    /// Represents a standard playing card with a suit and a value.
    /// </summary>
    public class RegularCard(Suits suit, Values value) : Card
    {
        public Suits Suit { get; } = suit;
        public Values Value { get; } = value;

        /// <summary>
        /// Returns a string representation of the card in the format "{Value} of {Suit}".
        /// </summary>
        /// <returns>A string representation of the card.</returns>
        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }

        /// <summary>
        /// Determines whether the current card is equal to another card by comparing suit and value.
        /// </summary>
        /// <param name="otherCard">The other card to compare against.</param>
        /// <returns>True if both cards have the same suit and value; otherwise false.</returns>
        public bool IsEqual(Card otherCard)
        {
            return otherCard is not null and RegularCard regularCard
                && this.Value == regularCard.Value && this.Suit == regularCard.Suit;
        }
    }
}
