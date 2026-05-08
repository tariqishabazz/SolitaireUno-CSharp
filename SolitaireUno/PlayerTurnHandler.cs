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

            playerDecision = playerDecision?.ToLower().Trim() ?? "";

            if (playerDecision != null)
            {
                if (int.TryParse(playerDecision, out int decisionAsNumber)) // If input is a number
                {
                    if (decisionAsNumber > 0 && decisionAsNumber <= player.Hand.Count) // Valid card index
                    {
                        Card potentialCard = player.Hand[decisionAsNumber - 1]; // Get selected card

                        if (GameMethods.ValidCard(potentialCard, logicCard, gameMode, suitEnforcement)) // Validate move
                        {
                            player.PlayCard(potentialCard); // Play the card
                            deck.AddToDiscardPile(potentialCard);

                            visualCard = potentialCard;

                            if (potentialCard is RegularCard)
                                logicCard = potentialCard; // Update current card

                            if (potentialCard is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                                return (true, $"You played: {potentialCard} and skipped the Computer!", potentialCard);

                            else if ((potentialCard is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (potentialCard is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
                                return (true, $"You played: {potentialCard}, so the Computer had to draw!", potentialCard);
                   
                            return (true, $" You played: {potentialCard}!", potentialCard);
                        }
                        
                        else
                        {
                            return (false, "That is not a valid move, please choose again", null);
                        }
                    }
                }
               
                else if (playerDecision == "p.u" || playerDecision == "pu" || playerDecision == "pick up" || playerDecision == "pickup") // Pick up
                {
                    if (deck.Length() > 0 || deck.Length() == 0 && !deck.deckReshuffled)
                    {
                        Card card = deck.DealCard()!; // Draw a card
                        player.PickupCard(card); // Add to hand

                        int playerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCard); // Check penalty

                        switch (playerPotentialPenaltyCount)
                        {
                            case > 0:
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
                                break;

                            default:
                                break;
                        }

                        return (true, "You decided to pick up!", null);
                    }

                    else if (deck.Length() == 0 && deck.deckReshuffled)
                    {
                        return (false, "Deck has already been reshuffled. Either pass or play!", null);
                    }

                }
                else if (playerDecision == "pass" || playerDecision == "p") // Pass
                {
                    if (deck.Length() > 0)
                    {
                        return (false, "The deck still has cards!\n Either pick up or play!", null);
                    }
                    else if(deck.Length() == 0 && !deck.deckReshuffled)
                    {
                        return (false, "The deck hasn't been reshuffled! You can still pick up!", null);   
                    }
                    else
                    {
                        return (true, "You decided to pass!", null);
                    }
                }
            }

            return(false, "You didn't a decision", null);
        }
    }
}
