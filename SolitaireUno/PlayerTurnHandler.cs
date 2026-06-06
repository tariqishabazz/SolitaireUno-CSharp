/*
 PlayerTurnHandler.cs

 Purpose:
 - Encapsulates human player turn handling: validating decisions, performing pickups,
   handling passes, and applying penalties.

 Commenting guideline applied:
 - File-level purpose header added to match the Home.razor.cs style.
*/

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
        /// <param name="playerDecision">Player input string representing their action.</param>
        /// <param name="gameMode">The current game mode for validation.</param>
        /// <param name="suitEnforcement">Whether suit enforcement is active.</param>
        /// <returns>Tuple indicating success, a UI message, and the played card if any.</returns>
        public (bool sucessfulMove, string message, Card? playedCard) HandleTurn(ref Card logicCard, ref Card visualCard, Card penaltyCard, string playerDecision, Player nextPlayer, GameSettings currentGameSettings)
        {
            playerDecision = playerDecision?.ToLower().Trim() ?? "";


            // ---------------- PLAYER WANTS TO PASS ---------------- //

            if (playerDecision == "pass")
            {
                if (deck.Length() > 0)
                    return (false, "The deck still has cards!\n Either pick up or play!", null);

                else if (deck.Length() == 0 && !deck.DeckReshuffled)
                    return (false, "The deck hasn't been reshuffled! You can still pick up!", null);

                else
                    return (true, "You decided to pass!", null);
            }


            // ---------------- PLAYER WANTS TO PICKUP ---------------- //

            if (playerDecision == "pickup")
            {
                if (deck.Length() > 0 || (deck.Length() == 0 && (!deck.DeckReshuffled || currentGameSettings.Mode is GameMode.Both)))
                    return HandlePickup(penaltyCard);

                else
                    return (false, "The deck is empty and has been reshuffled! You can't pick up, you must pass or play!", null);
            }



            // --------------- ENSURING DECISION IS VALID -------------- //

            Card? validCard = ValidateDecision(playerDecision, logicCard, currentGameSettings);

            if (validCard is null)
                return (false, "Invalid choice! Please choose a valid card, pickup or pass!", null);

            // --------------- PLAYS AND SETS CARD -------------- //

            player.PlayCard(validCard);
            deck.AddToDiscardPile(validCard);

            visualCard = validCard;

            if (validCard is RegularCard)
                logicCard = validCard;

            if (validCard is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
            {
                return (true, $"You played: {validCard} and skipped {nextPlayer.Name}!", validCard);
            }

            else if ((validCard is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (validCard is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
            {
                return (true, $"You played: {validCard}, so {nextPlayer.Name} had to draw!", validCard);
            }

            return (true, $" You played: {validCard}!", validCard);
        }


        // ================ ALL METHODS =============== //


        /// <summary>
        /// Handles the player's action to pick up a card in response to a penalty, dealing the appropriate number of
        /// cards and updating the player's hand accordingly.
        /// </summary>
        /// <param name="penaltyCard">The penalty card that triggered the pickup action. Determines the number of additional cards the player may
        /// need to pick up.</param>
        /// <returns>A tuple containing a value indicating whether the move was successful, a message describing the outcome, and
        /// the card played, if any. The played card is always null in this context.</returns>
        public (bool sucessfulMove, string message, Card? playedCard) HandlePickup(Card penaltyCard)
        {
            Card card = deck.DealCard()!;
            player.PickupCard(card);

            int playerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCard);

            if (playerPotentialPenaltyCount > 0)
            {
                int actualPickupCount = 0;

                for (int i = 0; i < playerPotentialPenaltyCount; i++)
                {
                    Card? additionalPenaltyCard = deck.DealCard();

                    if (additionalPenaltyCard is null)
                        break;

                    player.PickupCard(additionalPenaltyCard);
                    actualPickupCount++;
                }

                return (true, $"You decided to pick up and found the {penaltyCard}! You picked up {actualPickupCount} additional cards!", null);
            }

            return (true, "You decided to pick up!", null);
        }

        private Card? ValidateDecision(string playerDecision, Card logicCard, GameSettings currentGameSettings)
        {
            if (!int.TryParse(playerDecision, out int decisionAsNumber)) // IF THE DECISION ISNT A NUMBER
                return null;

            Card potentialCard = player.Hand[decisionAsNumber - 1];

            if (decisionAsNumber <= 0 || decisionAsNumber > player.Hand.Count) // IF DECISION ISNT WITHIN RANGE OF CARDS
                return null;

            if (!CardValidation.ValidCard(potentialCard, logicCard, currentGameSettings)) // IF DECISION ISNT A VALID MOVE
                return null;

            return potentialCard;
        }
    }
}

