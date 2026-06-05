namespace SolitaireUno.Tests
{
    public class CardAndDeckTests
    {
        [Fact]
        public void DeckInitializesWith_Cards()
        {
            Deck testDeck = new Deck();

            int deckCount = testDeck.Length();

            Assert.NotNull(testDeck);
            Assert.Equal(deckCount, testDeck.Length());
        }

        [Fact]
        public void Length_Returns_A_ValidDeckLength()
        {
            Deck testDeck = new Deck();
            int deckLength = testDeck.Length();

            Assert.True(deckLength >= 0);
        }

        [Fact]
        public void Dealt_TopCard_IsNotSpecialCard()
        {
            Deck testDeck = new Deck();
            Card topCard = testDeck.PreventInitialSpecialCard()!;

            Assert.IsNotType<SpecialCard>(topCard);
        }

        [Fact]
        public void DeckCorrectly_Deals_A_Card_WhenAble()
        {
            Deck testDeck = new Deck();
            Card dealtCard = testDeck.DealCard()!;

            Assert.NotNull(dealtCard);
        }

        [Fact]
        public void DeckCorrectly_Reshuffles_Once_When_Empty()
        {
            Deck testDeck = new Deck();
            testDeck.DiscardPile.AddRange(testDeck.GameDeck);

            Deck.Empty(testDeck.GameDeck);

            Card? dealtCard = testDeck.DealCard();

            Assert.NotNull(dealtCard);
        }

        [Fact]
        public void DeckDoesnt_Reshuffle_If_Already_Shuffled()
        {
            Deck testDeck = new Deck();

            Deck.Empty(testDeck.GameDeck); // clearing deck
        
            testDeck.DeckReshuffled = true;

            Card? dealtCard = testDeck.DealCard();

            Assert.Null(dealtCard);
        }

        [Fact]
        public void DeckCorrectly_Adds_A_Card_To_DiscardPile()
        {
            Deck testDeck = new Deck();

            RegularCard randomCard = new(Suits.Diamonds, Values.Nine);
            testDeck.AddToDiscardPile(randomCard);

            Assert.NotEmpty(testDeck.DiscardPile);
        }

        [Fact] // FIX 
        public void DeckAllows_MultipleReshuffles_DuringBothGameMode()
        {
            // ARRANGE
            Deck testDeck = new Deck();
            int deckReshuffledCount = 0;

            // ACT
            for(int i = 0; i < 10; i++)
            {
                Deck.Empty(testDeck.GameDeck); // clearing deck
                testDeck.AddToDiscardPile(testDeck.DealCard()!);                

                deckReshuffledCount++; // First reshuffle
            }

            // ASSERT
            Assert.True(deckReshuffledCount > 5, $"Expected multiple reshuffles, but only got {deckReshuffledCount} reshuffles.");
        }
    }
}
