using Xunit;
using SolitaireUno;
using System;
using System.IO;

namespace SolitaireUno.Tests
{
    public class CardAndDeckTests
    {
        [Fact]
        public void DeckReshufflesCorrectly()
        {
            // arrange
            Deck gameDeck = new Deck();
            int startingDeckLength = gameDeck.Length();

            // act
            for (int i = 0; i < startingDeckLength; i++)
            {
                Card? dealtCard = gameDeck.DealCard();
                
                if(dealtCard is not null)
                    gameDeck.AddToDiscardPile(dealtCard);
            }

            Card? rescueCard = gameDeck.DealCard();
            int reshuffledLength = gameDeck.Length();

            // assert
            Assert.Equal(56, reshuffledLength);
        }

        [Fact]
        public void InitialCardIsNotSpecialCard()
        {
            // arrange
            Deck gameDeck = new Deck();
            Card? dealtCard = gameDeck.DealCard();


            // act
            if (dealtCard is not null)
                GameMethods.PreventInitialSpecialCard(dealtCard);

            bool result = dealtCard is SpecialCard;

            // assert
            Assert.False(result);
        }
    }
}
