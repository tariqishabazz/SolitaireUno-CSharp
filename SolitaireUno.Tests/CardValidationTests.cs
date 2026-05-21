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
        public void ValidCard_ReturnsTrue_With_ValidPlay_In_AscendingMode()
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
        public void ValidCard_ReturnsTrue_With_ValidPlay_In_DescendingMode()
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
        public void SameColor_ReturnsTrue_With_CardsOf_SameColor()
        {
            // ARRANGE
            var redCard1 = new RegularCard(Suits.Hearts, Values.Ace);
            var redCard2 = new RegularCard(Suits.Diamonds, Values.Ace);
            var blackCard1 = new RegularCard(Suits.Spades, Values.Ace);
            var blackCard2 = new RegularCard(Suits.Clubs, Values.Ace);

            // ACT
            bool redCardResult = CardValidation.SameColor(redCard1, redCard2);
            bool blackCardResult = CardValidation.SameColor(blackCard1, blackCard2);

            // ASSERT
            Assert.True(redCardResult);
            Assert.True(blackCardResult);
        }

        [Fact]
        public void SameColor_ReturnsFalse_With_CardsOf_DifferingColors()
        {
            // ARRANGE
            var redCard1 = new RegularCard(Suits.Hearts, Values.Ace);
            var blackCard1 = new RegularCard(Suits.Spades, Values.Ace);

            // ACT
            bool cardResult = CardValidation.SameColor(redCard1, blackCard1);

            // ASSERT
            Assert.False(cardResult);
        }

        [Fact]
        public void WrapAround_Logic_ReturnsTrue_InDesc()
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
        public void WrapAround_Logic_ReturnsTrue_InAsc()
        {
            // ARRANGE
            var tableCard = new RegularCard(Suits.Hearts, Values.King);
            var potentialPlay = new RegularCard(Suits.Spades, Values.Ace);

            // ACT
            bool result = CardValidation.ValidCard(potentialPlay, tableCard, GameMode.Ascending, false);

            // ASSERT
            Assert.True(result);
        }
    }
}
