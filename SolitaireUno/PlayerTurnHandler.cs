// PlayerTurnHandler.cs — handles human player's turn logic (validate, pickup, pass, play).

// Handles the logic for a human player's turn, including pick-up, pass and play logic.

namespace SolitaireUno
{
    /// <summary>
    /// Handles the logic for a human player's turn, including pick-up, pass and play logic.
    /// </summary>
    public class PlayerTurnHandler(Player player, Deck deck)
    {
        /// <summary>
        /// Processes a player's turn based on input and returns the result along with any UI message and played card.
        /// </summary>
        /// <param name="logicCard">Reference to the logic card used for validation.</param>
        /// <param name="visualCard">Reference to the visual card shown to the UI.</param>
        /// <param name="penaltyCard">The configured penalty card.</param>
        /// <param name="playerDecision">Player input string representing their action (card index, "pickup", or "pass").</param>
        /// <param name="nextPlayer">The player who will act next (used for messages and penalty application).</param>
        /// <param name="currentGameSettings">Current game settings (mode, difficulty, suit enforcement, player count).</param>
        /// <returns>Tuple indicating whether the move was successful, a UI message, and the played card if any.</returns>
        public (bool sucessfulMove, string message, Card? playedCard) HandleTurn(List<RegularCard> penaltyCards, string playerDecision, Player nextPlayer, GameSettings currentGameSettings, GameState currentGameState)
        {
            int maxReshufflesGranted = currentGameSettings.Mode is GameMode.Both ? 2 : 1;
            playerDecision = playerDecision?.ToLower().Trim() ?? "";

            if (playerDecision.Equals("gimmeTheLastDiscard", StringComparison.OrdinalIgnoreCase))
            {
                if (deck.Length() > 0 || deck.ReshuffleCount < maxReshufflesGranted)
                    return (false, $"No reversing while deck has cards/can be reshuffled!", null);

                Card? lastDiscardedCard = deck.ReverseDiscard();

                if (lastDiscardedCard is null)
                    return (false, $"You cannot pull the only card in the discard pile!", null);

                if (deck.DiscardPile.TryPeek(out Card? previousCard))
                {
                    currentGameState.VisualCard = previousCard;
                    currentGameState.LogicCard = deck.DiscardPile.FirstOrDefault(card => card is RegularCard) ?? previousCard;
                }

                player.Hand.Add(lastDiscardedCard);
                return (true, $"Reverse Reverse! You pulled back the {lastDiscardedCard}!", null);
            }

            // Player wants to pass
            if (playerDecision == "pass")
            {
                if (deck.Length() > 0 || (deck.Length() == 0 && (deck.ReshuffleCount < maxReshufflesGranted)))
                    return (false, "The deck still has cards or can be reshuffled!\n Either pick up or play!", null);

                else
                    return (true, "You decided to pass!", null);
            }

            // Player wants to pick up a card
            if (playerDecision == "pickup")
            {
                if (deck.Length() > 0 || (deck.Length() == 0 && (deck.ReshuffleCount < maxReshufflesGranted)))
                    return HandlePickup(penaltyCards);

                else
                    return (false, "The deck is empty and has been reshuffled! You can't pick up, you must pass or play!", null);
            }

            // Validate the player's card choice
            Card? validCard = ValidateDecision(playerDecision, currentGameState.LogicCard, currentGameSettings, currentGameState.LeapFrogMode);

            if (validCard is null)
                return (false, "Invalid choice! Please choose a valid card, pickup or pass!", null);

            // Play the card and update game state
            player.PlayCard(validCard);
            deck.AddToDiscardPile(validCard);

            currentGameState.VisualCard = validCard;

            if (validCard is RegularCard)
                currentGameState.LogicCard = validCard;

            if (validCard is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
            {
                return (sucessfulMove: true, message: $"You played: {validCard} and skipped {nextPlayer.Name}!", playedCard: validCard);
            }

            else if ((validCard is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (validCard is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
            {
                return (true, $"You played: {validCard}, so {nextPlayer.Name} had to draw!", validCard);
            }

            return (true, $" You played: {validCard}!", validCard);
        }

        /// <summary>
        /// Handles the player's action to pick up a card in response to a penalty, dealing the appropriate number of
        /// cards and updating the player's hand accordingly.
        /// </summary>
        /// <param name="penaltyCard">The penalty card that triggered the pickup action. Determines the number of additional cards the player may
        /// need to pick up.</param>
        /// <returns>A tuple containing a value indicating whether the move was successful, a message describing the outcome, and
        /// the card played, if any. The played card is always null in this context.</returns>
        public (bool sucessfulMove, string message, Card? playedCard) HandlePickup(List<RegularCard> penaltyCards)
        {
            var (drawnCard, penaltyCount, actualPenaltyCardsDealt) = PickupProcessor.ProcessPickup(deck, player, penaltyCards);

            if (drawnCard is null)
                return (true, "The deck is empty! You must pass.", null);

            if (penaltyCount > 0)
                return (true, $"You decided to pick up and found the {drawnCard}! You picked up {actualPenaltyCardsDealt} additional card(s)!", null);

            return (true, "You decided to pick up!", null);
        }

        private Card? ValidateDecision(ReadOnlySpan<char> playerDecision, Card logicCard, GameSettings currentGameSettings, bool isLeapFrog)
        {
            if (!int.TryParse(playerDecision, out int decisionAsNumber))
                return null;

            if (decisionAsNumber <= 0 || decisionAsNumber > player.Hand.Count)
                return null;

            Card potentialCard = player.Hand[ decisionAsNumber - 1 ];

            if (!CardValidation.ValidCard(potentialCard, logicCard, currentGameSettings, isLeapFrog))
                return null;

            return potentialCard;
        }
    }
}

