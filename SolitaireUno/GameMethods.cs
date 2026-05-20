using SolitaireUno;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    /// <summary>
    /// Core game logic helpers used by the Solitaire Uno game.
    /// </summary>
    public class GameMethods
    {
        /// <summary>
        /// Returns the penalty count if the dealt regular card equals the penalty card; otherwise 0.
        /// </summary>
        public static int GetPenaltyCount(Card dealtCard, Card penaltyCard)
        {
            const int PenaltyCardCount = 4;

            if (dealtCard is RegularCard regularCard)
                return regularCard.IsEqual(penaltyCard) ? PenaltyCardCount : 0;

            return 0;
        }

        /// <summary>
        /// Returns the action instruction associated with a special card; regular cards return DoNothing.
        /// </summary>
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

        /// <summary>
        /// Applies special-card effects (skip/draw) to the target player and returns whether they were skipped.
        /// </summary>
        public static bool ApplySpecialCardEffect(Card? lastPlayedCard, bool targetSkipped, Deck gameDeck, Player targetPlayer, Card penaltyCard)
        {
            if (lastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(lastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        break;

                    case ActionInstruction.SkipTurn:
                        targetSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        ProcessDraw(4, targetPlayer, gameDeck, penaltyCard);
                        targetSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        ProcessDraw(2, targetPlayer, gameDeck, penaltyCard);
                        targetSkipped = true;
                        break;

                    default:
                        break;
                }
            }

            return targetSkipped;
        }

        /// <summary>
        /// Performs draw operations for penalties, handling chained penalties when penalty card is drawn.
        /// </summary>
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
                {
                    break;
                }
            }
        }
    }
}
