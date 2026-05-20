using SolitaireUno;
using Xunit;
using NUnit;

namespace SolitaireUno.Tests
{
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
    }
}