using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolitaireUno;

namespace SolitaireUno.Tests
{
    /// <summary>
    /// Contains unit tests for validating card play logic and color matching in various game modes.
    /// </summary>
    /// 
    /// <remarks>These tests verify the behavior of the CardValidation class, ensuring correct handling of
    /// valid and invalid plays, special cards, color matching, and wrap-around logic in both ascending and descending
    /// game modes. The tests are intended to ensure that the card validation logic adheres to the expected game
    /// rules.</remarks>
    public class CardValidationTests
    {
        [Fact]
        public void ValidCard_ReturnsTrue_With_ValidPlay_Asc()
        {
            // ARRANGE
            var cardOnTable = new RegularCard(Suits.Clubs, Values.Four);
            var potentialPlay = new RegularCard(Suits.Spades, Values.Five);

            // ACT
            bool validPlay = CardValidation.ValidCard(potentialPlay, cardOnTable, GameMode.Ascending, false);

            // ASSERT
            Assert.True(validPlay);
        }

        [Fact]
        public void ValidCard_ReturnsTrue_With_ValidPlay_Desc()
        {
            // ARRANGE
            var cardOnTable = new RegularCard(Suits.Clubs, Values.Seven);
            var potentialPlay = new RegularCard(Suits.Spades, Values.Six);

            // ACT
            bool validPlay = CardValidation.ValidCard(potentialPlay, cardOnTable, GameMode.Descending, false);

            // ASSERT
            Assert.True(validPlay);
        }

        [Fact]
        public void ValidCard_ReturnsFalse_With_NonValidPlay_AnyMode()
        {
            // ARRANGE
            var cardOnTable = new RegularCard(Suits.Hearts, Values.Eight);
            var potentialPlay = new RegularCard(Suits.Spades, Values.Six);

            // ACT
            bool validPlay = CardValidation.ValidCard(potentialPlay, cardOnTable, GameMode.Descending, false);

            // ASSERT
            Assert.False(validPlay);
        }

        [Fact]
        public void ValidCard_ReturnsTrue_With_AnySpecialCard_AnyMode()
        {
            // ARRANGE
            var cardOnTable = new RegularCard(Suits.Hearts, Values.Eight);

            // ACT & ASSERT
            // COME BACK TO THIS

        }

        [Fact]
        public void NotSameColor_ReturnsFalse_With_CardsOf_SameColor()
        {
            // ARRANGE
            var redCard1 = new RegularCard(Suits.Hearts, Values.Ace);
            var redCard2 = new RegularCard(Suits.Diamonds, Values.Ace);
            var blackCard1 = new RegularCard(Suits.Spades, Values.Ace);
            var blackCard2 = new RegularCard(Suits.Clubs, Values.Ace);

            // ACT
            bool redCardResult = CardValidation.NotSameColor(redCard1, redCard2);
            bool blackCardResult = CardValidation.NotSameColor(blackCard1, blackCard2);

            // ASSERT
            Assert.False(redCardResult);
            Assert.False(blackCardResult);
        }

        [Fact]
        public void NotSameColor_ReturnsTrue_With_CardsOf_DifferingColors()
        {
            // ARRANGE
            var redCard1 = new RegularCard(Suits.Hearts, Values.Ace);
            var blackCard1 = new RegularCard(Suits.Spades, Values.Ace);

            // ACT
            bool cardResult = CardValidation.NotSameColor(redCard1, blackCard1);

            // ASSERT
            Assert.True(cardResult);
        }

        [Fact]
        public void WrapAround_Logic_ReturnsTrue_Desc()
        {
            // ARRANGE
            var tableCard = new RegularCard(Suits.Hearts, Values.Ace);
            var potentialPlay = new RegularCard(Suits.Spades, Values.King);

            // ACT
            bool result = CardValidation.ValidCard(potentialPlay, tableCard, GameMode.Descending, false);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void WrapAround_Logic_ReturnsTrue_Asc()
        {
            // ARRANGE
            var tableCard = new RegularCard(Suits.Hearts, Values.King);
            var potentialPlay = new RegularCard(Suits.Spades, Values.Ace);

            // ACT
            bool result = CardValidation.ValidCard(potentialPlay, tableCard, GameMode.Ascending, false);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void SuitEnforcement_ReturnsTrue_ValidPlay_Asc()
        {
            // ARRANGE
            Card redTableCard = new RegularCard(Suits.Hearts, Values.Ten);
            Card blackPotentialCard = new RegularCard(Suits.Clubs, Values.Jack);

            // ACT
            bool result = CardValidation.ValidCard(blackPotentialCard, redTableCard, GameMode.Ascending, true);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void SuitEnforcement_ReturnsTrue_ValidPlay_Desc()
        {
            // ARRANGE
            Card redTableCard = new RegularCard(Suits.Hearts, Values.Jack);
            Card blackPotentialCard = new RegularCard(Suits.Clubs, Values.Ten);

            // ACT
            bool result = CardValidation.ValidCard(blackPotentialCard, redTableCard, GameMode.Descending, true);

            // ASSERT
            Assert.True(result);
        }
    }
}
