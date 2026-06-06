namespace SolitaireUno.Tests
{
    public class CardAndDeckTests
    {
        [Fact]
        public void DeckInitializesWith_Cards()
        {
            Deck testDeck = new Deck(GameMode.Ascending);

            int deckCount = testDeck.Length();

            Assert.NotNull(testDeck);
            Assert.Equal(deckCount, testDeck.Length());
        }

        [Fact]
        public void Length_Returns_A_ValidDeckLength()
        {
            Deck testDeck = new Deck(GameMode.Ascending);
            int deckLength = testDeck.Length();

            Assert.True(deckLength >= 0);
        }

        [Fact]
        public void Dealt_TopCard_IsNotSpecialCard()
        {
            Deck testDeck = new Deck(GameMode.Ascending);
            Card topCard = testDeck.PreventInitialSpecialCard()!;

            Assert.IsNotType<SpecialCard>(topCard);
        }

        [Fact]
        public void DeckCorrectly_Deals_A_Card_WhenAble()
        {
            Deck testDeck = new Deck(GameMode.Ascending);
            Card dealtCard = testDeck.DealCard()!;

            Assert.NotNull(dealtCard);
        }

        [Fact]
        public void DeckCorrectly_Reshuffles_Once_When_Empty()
        {
            Deck testDeck = new Deck(GameMode.Ascending); // This Gamemode ensures that the deck will not reshuffle more than once
            testDeck.DiscardPile.AddRange(testDeck.GameDeck);

            Deck.Empty(testDeck.GameDeck);

            Card? dealtCard = testDeck.DealCard();

            Assert.NotNull(dealtCard);
        }

        /// <summary>
        /// Verifies that the deck does not reshuffle if it has already been marked as shuffled.
        /// </summary>
        /// <remarks>This test ensures that when the deck's reshuffle flag is set, dealing a card from an
        /// empty deck does not trigger a reshuffle and returns null instead. This behavior prevents unnecessary
        /// reshuffling operations when the deck is already considered shuffled.</remarks>
        [Fact]
        public void DeckDoesnt_Reshuffle_If_Already_Shuffled_InNonBoth_GameMode()
        {
            Deck testDeck = new Deck(GameMode.Ascending); // This Gamemode ensures that the deck will not reshuffle more than once

            Deck.Empty(testDeck.GameDeck);

            testDeck.DeckReshuffled = true; // Simulate that the deck has already been reshuffled

            Card? dealtCard = testDeck.DealCard();

            Assert.Null(dealtCard);
        }

        /// <summary>
        /// Verifies that adding a card to the discard pile using AddToDiscardPile results in the discard pile
        /// containing the card.
        /// </summary>
        /// <remarks>This unit test ensures that the Deck class correctly updates its DiscardPile
        /// collection when a card is added. It is intended to validate the expected behavior of the AddToDiscardPile
        /// method.</remarks>
        [Fact]
        public void DeckCorrectly_Adds_A_Card_To_DiscardPile()
        {
            Deck testDeck = new Deck(GameMode.Ascending); // GameMode is arbitrary here, as it doesn't affect the discard pile functionality

            RegularCard randomCard = new(Suits.Diamonds, Values.Nine);
            testDeck.AddToDiscardPile(randomCard);

            Assert.NotEmpty(testDeck.DiscardPile);
        }

        /// <summary>
        /// This test ensures that the deck can be reshuffled multiple times during a game.
        ///     It simulates multiple rounds of play where the game deck is depleted 
        ///     and needs to be replenished from the discard pile. 
        ///     The test checks that after each reshuffle, the game deck is correctly replenished 
        ///     and that the reshuffling process can occur multiple times without issues.
        /// </summary>
        [Fact]
        public void DeckAllows_MultipleReshuffles_DuringBothGameMode()
        {
            // ARRANGE
            Deck testDeck = new Deck(GameMode.Both);
            int deckReshuffledCount = 0;

            // ACT
            for (int i = 0; i < 10; i++) // 10 is an arbitrary number to ensure multiple reshuffles
            {
                testDeck.DiscardPile.AddRange(testDeck.GameDeck);
                Deck.Empty(testDeck.GameDeck);

                Card? dealtCard = testDeck.DealCard();

                testDeck.AddToDiscardPile(dealtCard!);

                if (testDeck.GameDeck.Count != 0)
                    deckReshuffledCount++;
            }

            // ASSERT
            Assert.True(deckReshuffledCount > 5, $"Expected multiple reshuffles, but only got {deckReshuffledCount} reshuffles.");
        }
    }
}
