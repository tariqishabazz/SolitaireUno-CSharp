namespace SolitaireUno
{
    /// <summary>
    /// Handles pickup logic, penalty chaining, and card drawing for both player and computer.
    /// </summary>
    public static class PickupProcessor
    {
        /// <summary>
        /// Processes a pickup action: deals a card, checks for penalties, and deals additional penalty cards if needed.
        /// </summary>
        /// <param name="deck">The deck to draw cards from.</param>
        /// <param name="player">The player receiving the cards.</param>
        /// <param name="penaltyCards">The configured penalty cards to check against.</param>
        /// <returns>A tuple containing the card drawn, penalty count detected, and actual penalty cards dealt.</returns>
        public static (Card? drawnCard, int penaltyCount, int actualPenaltyCardsDealt) ProcessPickup(Deck deck, Player player, List<RegularCard> penaltyCards)
        {
            Card? drawnCard = deck.DealCard();

            if (drawnCard is null)
                return (null, 0, 0);

            player.PickupCard(drawnCard);

            int penaltyCount = PenaltyHandler.GetPenaltyCount(drawnCard, penaltyCards);
            int actualPenaltyCardsDealt = 0;

            if (penaltyCount > 0)
            {
                for (int i = 0; i < penaltyCount; i++)
                {
                    Card? penaltyCard = deck.DealCard();

                    if (penaltyCard is null)
                        break;

                    player.PickupCard(penaltyCard);
                    actualPenaltyCardsDealt++;
                }
            }

            return (drawnCard, penaltyCount, actualPenaltyCardsDealt);
        }
    }
}
