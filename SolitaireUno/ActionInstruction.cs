using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    public enum ActionInstruction
    {
        DoNothing,
        SkipTurn, 
        ChangeOrder, 
        DrawTwo, 
        DrawFour
    }
}
