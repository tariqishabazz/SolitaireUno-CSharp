using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    /// <summary>
    /// Represents available game modes.
    /// </summary>
    public enum GameMode { Ascending, Descending, AscendingAndDescending }

    /// <summary>
    /// Represents AI difficulty levels used to influence computer move selection.
    /// </summary>
    public enum GameDifficulty { Easy, Medium, Hard }

    public record struct GameSettings(GameMode Mode,
                                               GameDifficulty Difficulty,
                                               bool SuitsEnforced,
                                               int NumberOfPlayers);
}
