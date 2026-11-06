namespace SolitaireUno
{
    /// <summary>
    /// Encapsulates game rule enforcement: stalemate detection, leapfrog activation, and reshuffle limits.
    /// </summary>
    internal class GameRules(GameSettings gameSettings)
    {
        private readonly GameSettings _gameSettings = gameSettings;

        /// <summary>
        /// Checks for stalemate conditions and updates game state accordingly.
        /// </summary>
        /// <param name="cardPlayed">The card played this turn (null if passed/picked up).</param>
        /// <param name="handCountBefore">Player's hand count before the turn.</param>
        /// <param name="handCountAfter">Player's hand count after the turn.</param>
        /// <param name="deckLength">Current draw pile size.</param>
        /// <param name="deckReshuffleCount">Current reshuffle count.</param>
        /// <param name="gameState">Current game state to update.</param>
        public void CheckStalemateConditions(Card? cardPlayed, int handCountBefore, int handCountAfter, GameState gameState)
        {
            int maxReshuffles = GetMaxReshuffles(_gameSettings.Mode);
            bool isDeckDead = gameState.CurrentDeckSize == 0 && gameState.DeckReshuffleCount == maxReshuffles;

            // Track consecutive passes (when deck is dead and no card was played)
            gameState.ConsecutivePasses = (cardPlayed is null && isDeckDead) ? gameState.ConsecutivePasses + 1 : 0;

            // Track consecutive pickups (when a player picked up instead of playing)
            gameState.ConsecutivePickups = (cardPlayed is null && handCountBefore < handCountAfter) ? gameState.ConsecutivePickups + 1 : 0;

            // Activate leapfrog mode if stalemate thresholds are met
            gameState.LeapFrogMode = ShouldActivateLeapFrog(gameState, maxReshuffles);
        }

        /// <summary>
        /// Determines if leapfrog mode should be active based on current conditions.
        /// </summary>
        private bool ShouldActivateLeapFrog(GameState gameState, int maxReshuffles)
        {
            int reshuffleLimit = maxReshuffles;
            bool hitReshuffleLimit = reshuffleLimit == maxReshuffles;

            // * 2 REPRESENTS EVERYONE PICKING UP FOR TWO WHOLE ROUNDS
            return (hitReshuffleLimit && gameState.ConsecutivePasses == gameState.CurrentPlayers.Count) || gameState.ConsecutivePickups == (gameState.CurrentPlayers.Count * 2);
        }

        /// <summary>
        /// Gets the maximum number of times the deck can be reshuffled based on game mode.
        /// </summary>
        public int GetMaxReshuffles(GameMode mode) => mode == GameMode.Both ? 2 : 1;
    }
}