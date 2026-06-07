// GameConfiguration.cs — enums and record for game mode and difficulty.

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
