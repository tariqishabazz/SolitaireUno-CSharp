/*
 SpecialCardType.cs

 Purpose:
 - Enum for special Uno-style cards that alter the game state when played.

*/

namespace SolitaireUno
{
    /// <summary>
    /// Types of special cards that cause game effects.
    /// </summary>
    public enum SpecialCardType
    {
        /// <summary>
        /// Skips the next player's turn.
        /// </summary>
        Skip,

        /// <summary>
        /// Causes the target player to draw two cards.
        /// </summary>
        DrawTwo,

        /// <summary>
        /// Causes the target player to draw four cards.
        /// </summary>
        DrawFour,
    }
}
