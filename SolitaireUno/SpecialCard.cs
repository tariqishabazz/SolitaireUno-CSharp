// SpecialCard.cs — special cards that trigger game effects.

namespace SolitaireUno
{
    /// <summary>
    /// Represents a special card that triggers game effects like skip, draw, or change order (beta).
    /// </summary>
    public class SpecialCard(SpecialCardType specialCardType) : Card
    {
        /// <summary>
        /// Gets or sets the special card type which determines its effect.
        /// </summary>
        public SpecialCardType CardType { get; set; } = specialCardType;

        /// <summary>
        /// Returns the display string for the special card.
        /// </summary>
        /// <returns>A string representation of the special card type.</returns>
        public override string ToString()
        {
            return $"{CardType}";
        }
    }
}
