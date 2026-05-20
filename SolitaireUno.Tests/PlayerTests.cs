using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void PlayerCorrectly_IntializeWith_10Cards()
        {
            Deck testDeck = new Deck();

            Player player1 = new Player(testDeck);

            int initialPlayerHandCount = player1.Hand.Count;

            Assert.Equal(10, initialPlayerHandCount);
        }
    }
}
