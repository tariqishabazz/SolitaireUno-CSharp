namespace SolitaireUno.Tests
{
    public class EngineFullGameTests
    {
        [Theory]
        [InlineData(GameMode.Ascending, GameDifficulty.Hard, true, 3)]
        [InlineData(GameMode.Descending, GameDifficulty.Hard, true, 3)]
        [InlineData(GameMode.Both, GameDifficulty.Hard, true, 3)]
        [InlineData(GameMode.Ascending, GameDifficulty.Medium, true, 3)]
        [InlineData(GameMode.Descending, GameDifficulty.Medium, true, 3)]
        [InlineData(GameMode.Both, GameDifficulty.Medium, true, 3)]
        public void PlayFullGame_ToCompletion(GameMode mode, GameDifficulty difficulty, bool suitEnforced, int numberOfPlayers)
        {
            // Arrange
            var deck = new Deck(); // for determinism: construct a pre-made deck or add seed support
            var settings = new GameSettings(mode, difficulty, suitEnforced, numberOfPlayers);
            var game = new MainGame(deck, settings);

            game.StartGame();

            int maxTurns = 20000; // safety cap
            int turnCount = 0;

            // Act: loop until a player has no cards (someone wins)
            while (game.AllPlayers.All(p => p.Hand.Count > 0) && turnCount < maxTurns)
            {
                var currentPlayer = game.AllPlayers[game.CurrentTurnIndex];

                string decision;

                if (currentPlayer is Player) // human player
                {
                    // find first playable card index
                    var hand = currentPlayer.Hand;
                    int playableIndex = -1;

                    for (int i = 0; i < hand.Count; i++)
                    {
                        var card = hand[i];
                        if (CardValidation.ValidCard(card, game.LogicCard!, game.CurrentGameSettings))
                        {
                            playableIndex = i;
                            break;
                        }
                    }

                    if (playableIndex >= 0)
                    {
                        // UI expects 1-based index strings
                        decision = (playableIndex + 1).ToString();
                    }
                    else
                    {
                        // pickup if available, otherwise pass
                        decision = game.GameDeck.Length() > 0 ? "p.u" : "pass";
                    }
                }
                else
                {
                    // computer turn handled by engine when decision == ""
                    decision = "";
                }

                var (message, success) = game.AdvanceTurn(decision);

                turnCount++;
            }

            // Assert: somebody has an empty hand (game finished)
            Assert.True(game.AllPlayers.Any(p => p.Hand.Count == 0), "No player reached zero cards within the turn limit.");
        }
    }
}