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

        [Theory]
        [InlineData(3)]
        [InlineData(2)]
        [InlineData(1)]
        public void GameCorrectly_Initializes_Cards_BasedOn_PlayerCount(int numberOfPlayers)
        {
            // ARRANGE
            Deck testDeck = new Deck();
            GameSettings gameSettings = new GameSettings { NumberOfPlayers = numberOfPlayers };

            MainGame fakeGame = new MainGame(testDeck, gameSettings);

            Player fakePlayer = fakeGame.AllPlayers[0];

            // ACT 
            fakeGame.StartGame();
            int playerHandCount = fakePlayer.Hand.Count;

            // ASSERT
            if (numberOfPlayers == 1)
                Assert.Equal(10, playerHandCount);

            else if (numberOfPlayers == 2)
                Assert.Equal(7, playerHandCount);

            else
                Assert.Equal(5, playerHandCount);
        }
    }
}
