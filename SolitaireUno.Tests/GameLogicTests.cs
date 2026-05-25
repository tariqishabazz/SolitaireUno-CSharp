using Xunit;

namespace SolitaireUno.Tests
{
    public class GameLogicTests
    {

        [Fact]
        public static void AscendingLogicFunctional()
        {
            Card card1 = new RegularCard(Suits.Clubs, Values.Five);
            Card card2 = new RegularCard(Suits.Hearts, Values.Six);

            bool result = GameMethods.ValidCard(card2, card1, GameMode.Ascending);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public static void DescendingLogicFunctional()
        {
            // Arrange
            MainGame.PlayerGameModeChoice = "ascending";

            Card cardToPlay = new(Suits.Diamonds, Values.Seven);
            Card cardInPlay = new(Suits.Clubs, Values.Eight);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            Assert.True(result);
        }

        [Fact]
        public static void WrapAroundLogicFunctionalAscending()
        {
            // Arrange
            MainGame.PlayerGameModeChoice = "ascending";

            bool result = GameMethods.ValidCard(card1, card2, GameMode.Ascending);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public static void WrapAroundLogicFunctionalDescending()
        {
            // Arrange (Setting up the scenario)
            MainGame.PlayerGameModeChoice = "descending";

            bool result = GameMethods.ValidCard(card2, card1, GameMode.Descending);

            // Act (Running the method)
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            // Assert (Verifying the result)
            Assert.True(result);
        }

        [Fact]
        public static void IsSpecialCardLogicFunctional()
        {
            // Arrange
            MainGame.PlayerGameModeChoice = "descending";

            var cardToPlay = new Card(Suits.Diamonds, Values.King); // King on Ace
            var cardInPlay = new Card(Suits.Spades, Values.Ace);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            Assert.True(result);
        }

        [Fact]
        public static void PenaltyCountLogicFunctional()
        {
            // Arrange
            MainGame.PlayerGameModeChoice = "descending";

            Card cardToPlay = new Card(Suits.Clubs, Values.Ten);
            Card cardInPlay = new Card(Suits.Spades, Values.Nine);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay);

            int result = GameMethods.GetPenaltyCount(dealtCard, penaltyCard);

            // Assert
            Assert.Equal(5, result);
        }
        
        [Fact]
        public void GetPenaltyCount_DealtCardIsNotPenaltyCard()
        {
            // Arrange
            var penaltyCard = new Card(Suits.Spades, Values.Queen);
            var dealtCard = new Card(Suits.Clubs, Values.Two);

            // Act
            int result = GameMethods.GetPenaltyCount(dealtCard, penaltyCard);

            // Assert
            Assert.Equal(0, result);
        }
        
        [Fact]
        public void ControllingDeck()
        {
            // arrange
            var card1 = new Card(Suits.Spades, Values.Ace);
            var card2 = new Card(Suits.Hearts, Values.Two);

            List < Card > cards = new List<Card> { card1, card2 };

            // act
            Deck myDeck = new(cards);
            Card result = myDeck.DealCard()!;

            // assert
            Assert.Equal(card1, result);
        }
        
        [Fact]
        public void MockInputReturns_ScriptedMoves()
        {
            // arrange
            var robotMoves = new List<string> { "p.u", "1" };
            MockInput robot = new MockInput(robotMoves);

            // act and assert
            Assert.Equal("p.u", robot.GetInput());
            Assert.Equal("1", robot.GetInput());
            Assert.Equal("pass", robot.GetInput());
        }

        [Fact]
        public void FullGamePlayedInAscending()
        {
            // Arrange
            MainGame.PlayerGameModeChoice = "ascending";
            
            Computer player1 = new();
            Computer player2 = new();

            Deck gameDeck = new();
            Card currentCard;

            int turnCounter = 0;
            int maxTurns = 100;

            // Act


            // Assert


        }
    }
}
