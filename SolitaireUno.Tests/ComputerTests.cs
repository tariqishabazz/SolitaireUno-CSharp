namespace SolitaireUno.Tests
{
    /// <summary>
    /// Contains unit tests for verifying the behavior of the Computer class.
    /// </summary>
    public class ComputerTests
    {
        [Fact]
        public void Computer_Correctly_InitializesWith_10_Cards()
        {
            // ARRANGE
            Deck testDeck = new Deck();
            Computer computer = new Computer(testDeck);
            
            // ACT 
            int computerHandCount = computer.Hand.Count;
            
            // ASSERT
            Assert.Equal(10, computerHandCount);
        }
    }
}