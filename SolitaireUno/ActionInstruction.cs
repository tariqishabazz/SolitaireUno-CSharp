// ActionInstruction.cs — enum of high-level actions for special cards.

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
        /// Game direction goes the opposite way
        /// </summary>
        Reverse,

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
