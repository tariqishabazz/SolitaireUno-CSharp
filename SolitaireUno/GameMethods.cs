using SolitaireUno;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

//Console.OutputEncoding = System.Text.Encoding.UTF8;

namespace SolitaireUno
{
    /// <summary>
    /// This class contains all the crucial methods of the SolitaireUno experience.
    /// </summary>
    public class GameMethods
    {
        /// <summary>
        /// This determines if a dealt card is the penalty card.  
        /// </summary>
        /// <param name="dealtCard">The card dealt. </param>
        /// <param name="penaltyCard">The assigned penalty card.  </param>
        /// <returns></returns>
        public static int GetPenaltyCount(Card dealtCard, Card penaltyCard)
        { 
            // Here is the set penalty count if the dealt card is the penalty card. 
            const int PenaltyCardCount = 4;

            // If the dealt card is a regular card, AKA, not a special card, we call the is equal method to see if it is the penalty card.
            //      If it is, set the penalty count to PenaltyCardCount. If it's not, set it to 0.  
            if (dealtCard is RegularCard regularCard)
                return regularCard.IsEqual(penaltyCard) ? PenaltyCardCount : 0;
            
            // Return zero because special cards don't have penalties. 
            else
                return 0;
        }
        

        /// <summary>
        ///   This method returns an enum depending on the current card, if a special card was placed down.  
        ///   For instance, if draw four was put down. Return the draw four instruction. 
        ///   
        ///     If a regular card was put down then it returns the do nothing instruction. 
        /// </summary>
        /// <param name="currentCard">The card currently on the table. </param>
        /// <returns>An ActionInstruction depending on the card</returns>
        public static ActionInstruction SpecialCardAction(Card currentCard)
        {
            switch (currentCard)
            {
                case SpecialCard specialCard:
                    if (specialCard.CardType.Equals(SpecialCardType.Skip))
                        return ActionInstruction.SkipTurn;

                    else if (specialCard.CardType.Equals(SpecialCardType.DrawFour))
                        return ActionInstruction.DrawFour;

                    else
                        return ActionInstruction.DrawTwo;
                default:
                    return ActionInstruction.DoNothing;
            }
        }


        public static bool PotentialPlayerAction(Card? lastPlayedCard, bool computerSkipped, Deck gameDeck, Computer computer, Card penaltyCard)
        {
            if (lastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(lastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        break;

                    case ActionInstruction.SkipTurn:
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        ProcessDraw(4, computer, gameDeck, penaltyCard);
                        
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        ProcessDraw(2, computer, gameDeck, penaltyCard);

                        computerSkipped = true;
                        break;

                    default:
                        break;
                }
                ;
            }
            return computerSkipped;
        }
        
        public static bool PotentialComputerAction(Card? lastPlayedCard, bool playerSkipped, Deck gameDeck, Player player, Card penaltyCard)
        {
            if (lastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(lastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        break;

                    case ActionInstruction.SkipTurn:
                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        ProcessDraw(4, player, gameDeck, penaltyCard);

                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        ProcessDraw(2, player, gameDeck, penaltyCard);

                        playerSkipped = true;
                        break;

                    default:
                        break;
                }
                ;
            }
            return playerSkipped;
        }

        /// <summary>
        /// This performs the draw 2 and 4 actions on another player.
        /// </summary>
        /// <param name="drawAmount">how many cards should be initially drawn</param>
        /// <param name="unfortunateSoul">the unfortunate player subject to penalty</param>
        /// <param name="gameDeck">the deck to be drawn from</param>
        /// <param name="penaltyCard">the penalty card for a potential addtional penalty</param>
        private static void ProcessDraw(int drawAmount, Player unfortunateSoul, Deck gameDeck, Card penaltyCard)
        {
            for (int i = 0; i < drawAmount; i++)
            {
                Card? drawnCard = gameDeck.DealCard();

                if (drawnCard is not null)
                {
                    if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                    {
                        int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);

                        for (int j = 0; j < awardedPenalty; j++)
                        {
                            Card? penaltyDrawnCard = gameDeck.DealCard();
                            
                            if (penaltyDrawnCard is not null)
                                unfortunateSoul.PickupCard(penaltyDrawnCard);
                            
                            else
                                break;
                        }
                    }
                    unfortunateSoul.PickupCard(drawnCard);
                }
                
                else
                    break;
            }
        }
    }
}