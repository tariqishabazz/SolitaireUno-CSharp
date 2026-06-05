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

                if (deck.Length() > 0 || (deck.Length() == 0 && !deck.DeckReshuffled && currentGameSettings.Mode is not GameMode.Both))
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
            }



            // --------------- ENSURING DECISION IS VALID -------------- //

            if (!int.TryParse(playerDecision, out int decisionAsNumber)) // IF THE DECISION ISNT A NUMBER
                return (false, "That isn't a valid move, please try again", null);

            if (decisionAsNumber <= 0 || decisionAsNumber > player.Hand.Count) // IF DECISION ISNT WITHIN RANGE OF CARDS
                return (false, "Invalid card index", null);

            Card potentialCard = player.Hand[decisionAsNumber - 1];

            if (!CardValidation.ValidCard(potentialCard, logicCard, currentGameSettings)) // IF DECISION ISNT A VALID MOVE
                return (false, "That is not a valid move, please try again", null);


            // --------------- PLAYS AND SETS CARD -------------- //

            player.PlayCard(potentialCard);
            deck.AddToDiscardPile(potentialCard);

            visualCard = potentialCard;

            if (potentialCard is RegularCard)
                logicCard = potentialCard;

            if (potentialCard is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                return (true, $"You played: {potentialCard} and skipped {nextPlayer.Name}!", potentialCard);

            else if ((potentialCard is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (potentialCard is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
                return (true, $"You played: {potentialCard}, so {nextPlayer.Name} had to draw!", potentialCard);

            return (true, $" You played: {potentialCard}!", potentialCard);
        }
    }
}

