namespace SolitaireUno.Tests
{
    /// <summary>
    /// Contains unit tests for methods in the GameMethods class, verifying correct behavior for penalty card counting,
    /// card drawing, and action instruction logic in the context of a card game.
    /// </summary>
    /// 
    /// <remarks>These tests use the xUnit framework to ensure that GameMethods operates as expected under
    /// various scenarios, such as handling penalty cards and dealing cards to players. The tests serve as regression
    /// checks and usage examples for the GameMethods API.</remarks>
    public class GameMethodTests
    {
        [Fact]
        public void PenaltyCardCount_EqualFour_WhenPenaltyFound()
        {
            // ARRANGE
            var penaltyCard = new RegularCard(Suits.Spades, Values.Queen);
            List<Card> singleCardListForDeck = [penaltyCard];
            Deck singleCardDeck = new Deck(singleCardListForDeck);

            // ACT
            Card dealtCard = singleCardDeck.DealCard()!;
            int penaltyCount = GameMethods.GetPenaltyCount(dealtCard, penaltyCard);

            // ASSERT
            Assert.Equal(4, penaltyCount);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        public void ProcessDraw_ProperlyDeals_Appropriate_Cards(int numberOfCardsToBeDealt)
        {
            // ARRANGE
            Deck fakeDeck = new Deck();
            Player unfortunateSoul = new Player { Name = "James" };

            RegularCard penaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            // ACT
            fakeDeck.GameDeck.Remove(penaltyCard); // removing the penalty to prevent additional cards from being drawn
            GameMethods.ProcessDraw(numberOfCardsToBeDealt, unfortunateSoul, fakeDeck, penaltyCard);

            // ASSERT
            int playerHandCountAfterDraw = unfortunateSoul.Hand.Count;
            Assert.Equal(numberOfCardsToBeDealt, playerHandCountAfterDraw);
        }

        [Fact]
        public void ActionInstruction_Returns_Proper_InstructionBased_OnCurrentCard()
        {
            // ARRANGE
            // ACT
            // ASSERT
        }
    }
}