using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    public class ConsoleInput : IInputProvider
    {
        public string GetInput() => Console.ReadLine();
    }
}
