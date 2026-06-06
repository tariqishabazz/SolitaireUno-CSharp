/*
 ActionInstruction.cs

 Purpose:
 - Enum describing high-level actions that special cards instruct the game to perform.

 Commenting guideline applied:
 - File-level purpose header added to align with the project's documentation style.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Represents the possible actions that a special card can instruct the game to perform.
    /// </summary>
    public enum ActionInstruction
    {
        /// <summary>
        /// No special action associated with the card.
        /// </summary>
        DoNothing,

        /// <summary>
        /// Skip the next player's turn.
        /// </summary>
        SkipTurn,

        /// <summary>
        /// The target player must draw two cards.
        /// </summary>
        DrawTwo,

        /// <summary>
        /// The target player must draw four cards.
        /// </summary>
        DrawFour
    }
}
