namespace SolitaireUno.Tests
{
    public class ComputerTests
    {
        [Fact]
        public void Computer_Correctly_InitializesWith_10_Cards()
        {
            Deck testDeck = new Deck();

            Computer computer = new Computer(testDeck);

            int computerHandCount = computer.Hand.Count;

            Assert.Equal(10, computerHandCount);
        }
    }
}