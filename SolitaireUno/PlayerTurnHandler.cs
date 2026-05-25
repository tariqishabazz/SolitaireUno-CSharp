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
    public class PlayerTurnHandler(Player player, Deck deck, IInputProvider input, IOutputProvider output)
    {
        /// <summary>
        /// Handles the logic for a single human player turn, including input, validation, and card actions.
        /// </summary>
        /// <param name="currentCard">Reference to the current card in play (may be updated).</param>
        /// <param name="penaltyCard">The penalty card for special rules.</param>
        public Card? HandleTurn(ref Card logicCard, ref Card visualCard, Card penaltyCard)
        {
            bool playerChoiceValid = false; // Tracks if the player made a valid move

            while (!playerChoiceValid)
            {
                // Show current card and deck info ONCE at the start of the turn
                GameMethods.ShowRoundSummary(visualCard, logicCard);
                GameMethods.ShowHand();

                output.WriteLine("---------------------------------------------------------------------");
                output.Write("\nPlay a Card \n(1 - Maximum # of Cards in Hand), Pick-Up (p.u), or Pass (p)... "); // Prompt for action
                
                string playerDecision = input.GetInput().ToLower(); // Get and normalize input

                if (playerDecision != null)
                {
                    if (int.TryParse(playerDecision, out int decisionAsNumber)) // If input is a number
                    {
                        if (decisionAsNumber > 0 && decisionAsNumber <= player.Hand.Count) // Valid card index
                        {
                            Card potentialCard = player.Hand[decisionAsNumber - 1]; // Get selected card

                            if (GameMethods.ValidCard(potentialCard, logicCard, MainGame.GameModeChoice)) // Validate move
                            {
                                player.PlayCard(potentialCard); // Play the card
                                deck.AddToDiscardPile(potentialCard);
                                
                                visualCard = potentialCard;

                                if (potentialCard is RegularCard)
                                {
                                    logicCard = potentialCard; // Update current card
                                }

                                output.WriteLine("---------------------------------------------------------------------");
                                output.WriteLine($"\nYou played {potentialCard}, so...");
                                
                                playerChoiceValid = true; // End turn

                                return potentialCard;
                            }
                            else
                            {
                                output.WriteLine("\n---------------------------------------------------------------------");
                                output.WriteLine("\nThat is not a valid play, please choose again"); // Invalid move
                            }
                        }
                        else
                        {
                            output.WriteLine("\n---------------------------------------------------------------------");
                            output.WriteLine("\nThat is an invalid input based on your current cards, please choose again."); // Invalid index
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

                                    output.WriteLine("\n---------------------------------------------------------------------");
                                    output.WriteLine("\nYou decided to pick up and received the Queen of Spades! [Insert Evil Laugh]");

                                    for (int i = 0; i < playerPotentialPenaltyCount; i++) // Add penalty cards
                                    {
                                        Card? additionalPenaltyCard = deck.DealCard();

                                        if (additionalPenaltyCard is not null)
                                        {
                                            player.PickupCard(additionalPenaltyCard);
                                            actualPickupCount++;
                                        }
                                    } 
                                                                        
                                    output.WriteLine($"You received {actualPickupCount} additional cards!");

                                    break;
                                
                                default:
                                    output.WriteLine("---------------------------------------------------------------------");
                                    output.WriteLine("\nYou decided to pick up so...");
                                    
                                    break;
                            }

                          
                            playerChoiceValid = true; // End turn
                            return null;
                        }

                        else if(deck.Length() == 0 && deck.deckReshuffled)
                        {
                            output.WriteLine("\n---------------------------------------------------------------------");
                            output.WriteLine("\nThere are no more cards in the deck, either pass or play!");
                        }
                        
                    }
                    else if (playerDecision == "pass" || playerDecision == "p") // Pass
                    {
                        if (deck.Length() > 0)
                        {
                            output.WriteLine("\n---------------------------------------------------------------------");
                            output.WriteLine("\nThere are still cards in the deck, either play or pick up!"); // Can't pass yet
                        }
                        else
                        {
                            output.WriteLine("\n---------------------------------------------------------------------");
                            output.WriteLine("\nYou decided to pass so...");
                            playerChoiceValid = true; // End turn
                        }
                    }
                    else
                    {
                        output.WriteLine("\n---------------------------------------------------------------------");
                        output.WriteLine("\nThat is not a valid decision, please try again");
                    }
                }
                else
                {
                    output.WriteLine("\n---------------------------------------------------------------------");
                    output.WriteLine("\nYou did not make a decision, please try again"); // No input
                }
            }
            return null;
        }
    }
}
