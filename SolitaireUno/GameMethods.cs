namespace SolitaireUno
{
    /// <summary>
    /// Core game logic helpers for draw processing with penalty chaining and UI messages.
    /// </summary>
    public static class GameMethods
    {
        /// <summary>
        /// Performs draw operations for penalties, handling chained penalties when penalty card is drawn.
        /// </summary>
        /// <param name="drawAmount">Number of cards to draw.</param>
        /// <param name="unfortunateSoul">The player who must pick up the cards.</param>
        /// <param name="gameDeck">The deck used to draw cards from.</param>
        /// <param name="penaltyCards">The configured penalty cards used to detect chained penalties.</param>
        /// <returns>An optional UI message describing the penalty outcome.</returns>
        public static string? ProcessDraw(int drawAmount, Player unfortunateSoul, Deck gameDeck, GameState gameState)
        {
            bool penaltyCardSpotted = false;
            int awardedPenaltyCount = 0;
            Card? drewCard = null;

            for (int i = 0; i < drawAmount; i++)
            {
                Card? drawnCard = gameDeck.DealCard();

                if (drawnCard is null)
                    break;

                int awardedPenalty = PenaltyHandler.GetPenaltyCount(drawnCard, gameState);

                if (awardedPenalty > 0)
                {
                    awardedPenaltyCount = awardedPenalty;
                    penaltyCardSpotted = true;
                    drewCard = drawnCard;

                    for (int j = 0; j < awardedPenalty; j++)
                    {
                        Card? penaltyDrawnCard = gameDeck.DealCard();

                        if (penaltyDrawnCard is null)
                            break;

                        unfortunateSoul.PickupCard(penaltyDrawnCard);
                    }
                }

                unfortunateSoul.PickupCard(drawnCard);
            }

            if (!penaltyCardSpotted)
                return null;

            return FormatDrawMessage(unfortunateSoul, drewCard, awardedPenaltyCount);
        }

        /// <summary>
        /// Formats a UI message describing the draw and penalty outcome.
        /// </summary>
        private static string FormatDrawMessage(Player player, Card? drewCard, int penaltyCount)
        {
            if (player is Computer)
                return $" During the draw, {player.Name} picked up the {drewCard}. Along with the normal draw, they will receive {penaltyCount} additional card(s).";
            else
                return $" During the draw, you picked up the {drewCard}. Along with the normal draw, you will receive {penaltyCount} additional card(s).";
        }
    }
}