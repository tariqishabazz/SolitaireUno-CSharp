using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno.Tests
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the Player class.
    /// </summary>
    /// <remarks>This test class uses the xUnit testing framework to ensure that the Player class initializes
    /// and functions as expected.</remarks>
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
