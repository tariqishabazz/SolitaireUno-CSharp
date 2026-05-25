using Xunit; // The testing framework
using SolitaireUno; // Giving access to SolitaireUno Code

namespace SolitaireUno.Tests
{
    public class GameLogicTests
    {
        [Fact]
        public void ValidCardTrue_CardOneValueHigherInAscendingMode()
        {
            // Arrange
            string gameMode = "ascending";
            
            Card cardInPlay = new(Suits.Hearts, Values.Four);
            Card cardToPlay = new(Suits.Spades, Values.Five);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void VaidCardFalse_CardOneValueLowerInAscendingMode()
        {
            // Arrange
            string gameMode = "ascending";

            Card cardToPlay = new(Suits.Diamonds, Values.Seven);
            Card cardInPlay = new(Suits.Clubs, Values.Eight);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void ValidCardTrue_AcePlayedOnKingInAscendingMode()
        {
            // Arrange
            string gameMode = "ascending";

            Card cardInPlay = new(Suits.Hearts, Values.King);
            Card cardToPlay = new(Suits.Diamonds, Values.Ace);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert
            Assert.True(result);
        }

        [Fact] // A "Fact" is a test that is always true, like facts irl
        public void ValidCardTrue_CardOneValueLowerInDescendingMode()
        {
            // Arrange (Setting up the scenario)
            string gameMode = "descending";

            var cardToPlay = new Card(Suits.Clubs, Values.Three); // 3 on 4
            var cardInPlay = new Card(Suits.Hearts, Values.Four);

            // Act (Running the method)
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert (Verifying the result)
            Assert.True(result);
        }

        [Fact]
        public void ValidCardTrue_KingPlayedOnAceInDescendingMode()
        {
            // Arrange
            string gameMode = "descending";

            var cardToPlay = new Card(Suits.Diamonds, Values.King); // King on Ace
            var cardInPlay = new Card(Suits.Spades, Values.Ace);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidCardFalse_CardIsHigherInDescendingMode()
        {
            // Arrange
            string gameMode = "descending";

            Card cardToPlay = new Card(Suits.Clubs, Values.Ten);
            Card cardInPlay = new Card(Suits.Spades, Values.Nine);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, gameMode);

            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void GetPenaltyCount_DealtCardEqualsPenaltyCard()
        {
            // Arrange
            var penaltyCard = new Card(Suits.Spades, Values.Queen);
            var dealtCard = new Card(Suits.Spades, Values.Queen);

            // Act
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
    }
}
