namespace SolitaireUno
{
    /// <summary>
    /// Handles penalty card detection and penalty count calculation.
    /// </summary>
    public static class PenaltyHandler
    {
        /// <summary>
        /// Returns the penalty count if the dealt regular card equals a penalty card; otherwise 0.
        /// </summary>
        /// <param name="dealtCard">The card that was drawn or dealt to a player.</param>
        /// <param name="gameState">The current game state containing penalty cards.</param>
        /// <returns>The number of penalty cards to apply (0 when none, random 1-5 when matched).</returns>
        public static int GetPenaltyCount(Card dealtCard, GameState gameState)
        {
            if (dealtCard is not RegularCard regularCard)
                return 0;

            if (gameState.PenaltyCards is null || gameState.PenaltyCards.Count == 0)
                return 0;

            foreach (RegularCard penaltyCard in gameState.PenaltyCards)
            {
                if (regularCard.IsEqual(penaltyCard))
                    return Random.Shared.Next(1, 6);
            }

            return 0;
        }

        /// <summary>
        /// Checks if a card is a penalty card.
        /// </summary>
        /// <param name="card">The card to check.</param>
        /// <param name="gameState">The current game state containing penalty cards.</param>
        /// <returns>True if the card matches a penalty card; otherwise false.</returns>
        public static bool IsPenaltyCard(Card card, GameState gameState)
        {
            if (card is not RegularCard regularCard)
                return false;

            if (gameState.PenaltyCards is null || gameState.PenaltyCards.Count == 0)
                return false;

            return gameState.PenaltyCards.Any(penaltyCard => regularCard.IsEqual(penaltyCard));
        }
    }
}