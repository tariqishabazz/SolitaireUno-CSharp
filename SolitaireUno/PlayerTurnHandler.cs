using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    /// <summary>
    /// Handles the logic for a human player's turn, including input, validation, and card actions.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the PlayerTurnHandler class.
    /// </remarks>
    /// <param name="player">The human player.</param>
    /// <param name="deck">The deck used for drawing cards.</param>
    /// <param name="input">Input provider for user input.</param>
    /// <param name="output">Output provider for user output.</param>
    public class PlayerTurnHandler(Player player, Deck deck)
    {
        /// <summary>
        /// Handles the logic for a single human player turn, including input, validation, and card actions.
        /// </summary>
        /// <param name="currentCard">Reference to the current card in play (may be updated).</param>
        /// <param name="penaltyCard">The penalty card for special rules.</param>
        public (bool sucessfulMove, string message, Card? playedCard) HandleTurn(ref Card logicCard, ref Card visualCard, Card penaltyCard, string playerDecision, GameMode gameMode, bool suitEnforcement)
        {
            // playerDecision?. : Checks if the player's input actually exists before trying to do anything to it.
            // ?? "" : A safety net that says, "If the input was missing or null,
                // just give me a blank text string ("") instead of crashing the game."
            
            playerDecision = playerDecision?.ToLower().Trim() ?? "";

            // -------------------- executes pass logic -----------------------
            
            if (playerDecision == "pass" || playerDecision == "p")
            {
                if (deck.Length() > 0)
                    return (false, "The deck still has cards!\n Either pick up or play!", null);

                else if (deck.Length() == 0 && !deck.deckReshuffled)
                    return (false, "The deck hasn't been reshuffled! You can still pick up!", null);

                else
                    return (true, "You decided to pass!", null);
            }

            // -------------------- executes pickup logic -----------------------

            else if (playerDecision == "p.u" || playerDecision == "pu" || playerDecision == "pick up" || playerDecision == "pickup") // Pick up
            {
                if (deck.Length() > 0 || deck.Length() == 0 && !deck.deckReshuffled)
                {
                    Card card = deck.DealCard()!;
                    player.PickupCard(card);

                    int playerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCard);

                    if (playerPotentialPenaltyCount != 0)
                    {
                        int actualPickupCount = 0;

                        for (int i = 0; i < playerPotentialPenaltyCount; i++) // Add penalty cards
                        {
                            Card? additionalPenaltyCard = deck.DealCard();

                            if (additionalPenaltyCard is not null)
                            {
                                player.PickupCard(additionalPenaltyCard);
                                actualPickupCount++;
                            }
                        }

                        return (true, $"You decided to pick up and found the {penaltyCard}! You picked up {actualPickupCount} additional cards!", null);
                    }

                    return (true, "You decided to pick up!", null);
                }

                else if (deck.Length() == 0 && deck.deckReshuffled)
                    return (false, "Deck has already been reshuffled. Either pass or play!", null);
            }


            // -------------------- if decision doesn't return a number -----------------------

            if (!int.TryParse(playerDecision, out int decisionAsNumber))
                return (false, "That isn't a valid move, please try again", null);

            // -------------------- if that number isn't within a valid range of cards -----------------------

            if (decisionAsNumber <= 0 || decisionAsNumber > player.Hand.Count)
                return (false, "Invalid card index", null);


            Card potentialCard = player.Hand[decisionAsNumber - 1];

            // -------------------- if ValidCard Returns false -----------------------

            if (!CardValidation.ValidCard(potentialCard, logicCard, gameMode, suitEnforcement))
                return (false, "That is not a valid move, please try again", null);


            // -------------------- Executes Play -----------------------

            player.PlayCard(potentialCard); 
            deck.AddToDiscardPile(potentialCard);

            visualCard = potentialCard;

            if (potentialCard is RegularCard)
                logicCard = potentialCard;

            if (potentialCard is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                return (true, $"You played: {potentialCard} and skipped the Computer!", potentialCard);

            else if ((potentialCard is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (potentialCard is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
                return (true, $"You played: {potentialCard}, so the Computer had to draw!", potentialCard);

            return (true, $" You played: {potentialCard}!", potentialCard);
        }
    }
}
