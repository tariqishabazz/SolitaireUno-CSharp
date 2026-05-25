using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    public static class GameSetup
    {
        public static void SetupGame(Player player, Computer computer, Deck deck)
        {
            for (int i = 0; i < 10; i++)
            {
                Card playerCard = deck.DealCard()!;
                player.PickupCard(playerCard);
                
                Card computerCard = deck.DealCard()!;
                computer.PickupCard(computerCard);
            }
        }
    }
}
