using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
