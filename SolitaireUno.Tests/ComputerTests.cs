using Xunit;

namespace SolitaireUno.Tests
{
    public class ComputerTests
    {
        [Fact]
        public static void NullWhenNoMoveFound()
        {
            Card currentCard = new RegularCard(Suits.Clubs, Values.Two);
            Card cardInHand = new RegularCard(Suits.Diamonds, Values.Seven);

            Computer computer = new Computer();
            computer.PickupCard(cardInHand);

            Card? potentialPlay = computer.MakeMove(currentCard, 5, 0, GameDifficulty.Easy);
           
            Assert.Null(potentialPlay);
        }

        [Fact]
        public static void NotNullWhenMoveFoundAsc()
        {
            MainGame.GameModeChoice = GameMode.Ascending;

            Card currentCard = new RegularCard(Suits.Clubs, Values.Two);
            Card cardInHand = new RegularCard(Suits.Diamonds, Values.Three);

            Computer computer = new Computer();
            computer.PickupCard(cardInHand);

            Card? potentialPlay = computer.MakeMove(currentCard, 5, 0, GameDifficulty.Easy);

            Assert.NotNull(potentialPlay);
        }

        [Fact]
        public static void NotNullWhenMoveFoundDesc()
        {
            MainGame.GameModeChoice = GameMode.Descending;

            Card currentCard = new RegularCard(Suits.Clubs, Values.Two);
            Card cardInHand = new RegularCard(Suits.Diamonds, Values.Ace);

            Computer computer = new Computer();
            computer.PickupCard(cardInHand);

            Card? potentialPlay = computer.MakeMove(currentCard, 5, 0, GameDifficulty.Easy);

            Assert.NotNull(potentialPlay);
        }
    }
}