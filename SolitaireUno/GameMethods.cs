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
        /// <param name="dealtCard">The card that was drawn or dealt to a player.</param>
        /// <param name="penaltyCard">The configured penalty card to compare against.</param>
        /// <returns>The number of penalty cards to apply (0 when none).</returns>
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
        /// <param name="currentCard">The card being evaluated for a special action.</param>
        /// <returns>The action instruction that should be applied based on the card type.</returns>
        public static ActionInstruction SpecialCardAction(Card currentCard) => currentCard switch
        {
            SpecialCard { CardType: SpecialCardType.Skip } => ActionInstruction.SkipTurn,
            SpecialCard { CardType: SpecialCardType.DrawTwo } => ActionInstruction.DrawTwo,
            SpecialCard { CardType: SpecialCardType.DrawFour } => ActionInstruction.DrawFour,
            _ => ActionInstruction.DoNothing
        };

        /// <summary>
        /// Applies special-card effects (skip/draw) to the target player and returns whether they were skipped.
        /// </summary>
        /// <param name="lastPlayedCard">The last card that was played which may trigger an effect.</param>
        /// <param name="targetSkipped">A flag indicating whether the target player is already skipped.</param>
        /// <param name="gameDeck">The deck used to draw penalty cards from.</param>
        /// <param name="targetPlayer">The player who will receive any drawn cards or be skipped.</param>
        /// <param name="penaltyCard">The configured penalty card used to detect chained penalties.</param>
        /// <returns>True if the target player was skipped as a result of the effect; otherwise false.</returns>
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
        /// <param name="drawAmount">Number of cards to draw.</param>
        /// <param name="unfortunateSoul">The player who must pick up the cards.</param>
        /// <param name="gameDeck">The deck used to draw cards from.</param>
        /// <param name="penaltyCard">The configured penalty card used to detect chained penalties.</param>
        public static void ProcessDraw(int drawAmount, Player unfortunateSoul, Deck gameDeck, Card penaltyCard)
        {
            for (int i = 0; i < drawAmount; i++)
            {
                Card? drawnCard = gameDeck.DealCard();

                if (drawnCard is null)
                    break;

                int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);
                
                if (awardedPenalty > 0)
                {
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
        }
    }
}
