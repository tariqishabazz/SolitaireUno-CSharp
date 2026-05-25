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

            // Assert
            Assert.True(result);
        }

        [Fact]
        public static void DescendingLogicFunctional()
        {
            // Arrange
            RegularCard cardToPlay = new(Suits.Diamonds, Values.Seven);
            RegularCard cardInPlay = new(Suits.Clubs, Values.Eight);

            // Act
            bool result = GameMethods.ValidCard(cardToPlay, cardInPlay, GameMode.Descending);

            Assert.True(result);
        }

        [Fact]
        public static void WrapAroundLogicFunctionalAscending()
        {
            // Arrange
            RegularCard card1 = new(Suits.Diamonds, Values.Ace);
            RegularCard card2 = new(Suits.Diamonds, Values.King);

            // Act
            bool result = GameMethods.ValidCard(card1, card2, GameMode.Ascending);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public static void WrapAroundLogicFunctionalDescending()
        {
            // Arrange (Setting up the scenario)
            RegularCard card1 = new(Suits.Diamonds, Values.King);
            RegularCard card2 = new(Suits.Diamonds, Values.Ace);

            // Act (Running the method)
            bool result = GameMethods.ValidCard(card1, card2, GameMode.Descending);

            // Assert (Verifying the result)
            Assert.True(result);
        }

        /* COME BACK TO THIS

        [Fact]
        public static void IsSpecialCardLogicFunctional()
        {                
            // Arrange

            List<Card> allSpecialCards = [];

            foreach (var specialCard in Enum.GetValues<SpecialCardType>())
            {
                allSpecialCards.Add(specialCard);
            }

            for (int i = 0; i < allSpecialCards.Count; i++)
            {
                bool result = GameMethods.IsSpecialCard(allSpecialCards[i]);
                Assert.True(result);
            }

            
        }

        */

        [Fact]
        public static void PenaltyCountLogicFunctional()
        {
            // Arrange
            RegularCard dealtCard = new(Suits.Spades, Values.Queen);
            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);

            // Act
            int result = GameMethods.GetPenaltyCount(dealtCard, penaltyCard);

            // Assert
            Assert.Equal(4, result);
        }

        [Fact]
        public void GetPenaltyCount_DealtCardIsNotPenaltyCard()
        {
            // Arrange
            RegularCard penaltyCard = new(Suits.Spades, Values.Queen);
            RegularCard dealtCard = new(Suits.Clubs, Values.Two);

            // Act
            int result = GameMethods.GetPenaltyCount(dealtCard, penaltyCard);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ControllingMockDeck()
        {
            // arrange
            RegularCard card1 = new(Suits.Spades, Values.Ace);
            RegularCard card2 = new(Suits.Hearts, Values.Two);

            List<Card> cards = new List<Card> { card1, card2 };

            // act
            Deck myDeck = new(cards);
            Card result = myDeck.DealCard()!;

            // assert
            Assert.Equal(card1, result);
        }

    }
}
