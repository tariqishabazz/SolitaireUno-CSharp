using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolitaireUno;

namespace SolitaireUno.Tests
{
    public class MainGameTests
    {
        [Theory]
        [InlineData(1, 1, 4, 2)]
        [InlineData(3, 1, 4, 0)]
        [InlineData(3, 2, 4, 1)]
        public void TurnIndex_LoopsProperly_OnLastPLayer(int currentIndex, int stepsMoved, int numberOfPlayers, int expectedNextIndex)
        {
            // ARRANGE && ACT
            int calculatedIndex = (currentIndex + stepsMoved) % numberOfPlayers;

            // ASSERT
            Assert.Equal(expectedNextIndex, calculatedIndex);

        }
    
    
    }
}
