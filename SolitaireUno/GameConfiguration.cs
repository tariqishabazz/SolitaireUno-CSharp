/*
 GameConfiguration.cs

 Purpose:
 - Defines game-level configuration types used across the application: game modes, AI difficulties, and the immutable GameSettings record.

 Commenting guideline applied:
 - File-level purpose header added to align with Home.razor.cs style.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Represents available game modes.
    /// </summary>
    public enum GameMode { Ascending, Descending, Both }

    /// <summary>
    /// Represents AI difficulty levels used to influence computer move selection.
    /// </summary>
    public enum GameDifficulty { Easy, Medium, Hard }

    public readonly record struct GameSettings(GameMode Mode, GameDifficulty Difficulty, bool SuitsEnforced, int NumberOfPlayers);
}
