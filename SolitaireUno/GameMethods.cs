// GameMethods.cs — stateless helper methods for penalties and special-card processing.

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
        /// <param name="penaltyCards">The configured penalty cards to compare against.</param>
        /// <returns>The number of penalty cards to apply (0 when none).</returns>
        public static int GetPenaltyCount(Card dealtCard, List<RegularCard> penaltyCards)
        {
            if (dealtCard is not RegularCard regularCard)
                return 0;

            foreach (RegularCard penaltyCard in penaltyCards)
            {
                if (regularCard.IsEqual(penaltyCard))
                    return Random.Shared.Next(1, 6); // return a random number between 1 and 5 for extra evilness
            }

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
            SpecialCard { CardType: SpecialCardType.Reverse } => ActionInstruction.Reverse,
            _ => ActionInstruction.DoNothing
        };

        /// <summary>
        /// Applies special-card effects (skip/draw) to the target player and returns whether they were skipped.
        /// </summary>
        /// <param name="lastPlayedCard">The last card that was played which may trigger an effect.</param>
        /// <param name="targetSkipped">A flag indicating whether the target player is already skipped.</param>
        /// <param name="gameDeck">The deck used to draw penalty cards from.</param>
        /// <param name="targetPlayer">The player who will receive any drawn cards or be skipped.</param>
        /// <param name="penaltyCards">The configured penalty cards used to detect chained penalties.</param>
        /// <returns>A tuple containing an optional message and a flag indicating if the target player was skipped.</returns>
        public static (string? potentialMessage, bool targetSkipped, bool isDirectionReversed) ApplySpecialCardEffect(Card? lastPlayedCard, bool targetSkipped, Deck gameDeck, Player targetPlayer, List<RegularCard> penaltyCards, int playerCount)
        {
            if (lastPlayedCard is null)
                return (null, targetSkipped, false);

            return SpecialCardAction(lastPlayedCard) switch
            {
                ActionInstruction.SkipTurn => (null, true, false),
                ActionInstruction.DrawTwo => (ProcessDraw(2, targetPlayer, gameDeck, penaltyCards), true, false),
                ActionInstruction.DrawFour => (ProcessDraw(4, targetPlayer, gameDeck, penaltyCards), true, false),
                ActionInstruction.Reverse => playerCount == 2 ? (null, true, false) : (null, false, true), // IF ONLY 2 PLAYERS, REVERSE ACTS AS A NORMAL SKIP
                _ => (null, targetSkipped, false)
            };
        }

        /// <summary>
        /// Performs draw operations for penalties, handling chained penalties when penalty card is drawn.
        /// </summary>
        /// <param name="drawAmount">Number of cards to draw.</param>
        /// <param name="unfortunateSoul">The player who must pick up the cards.</param>
        /// <param name="gameDeck">The deck used to draw cards from.</param>
        /// <param name="penaltyCards">The configured penalty cards used to detect chained penalties.</param>
        public static string? ProcessDraw(int drawAmount, Player unfortunateSoul, Deck gameDeck, List<RegularCard> penaltyCards)
        {
            bool penaltyCardSpotted = false;
            int awardedPenaltyCount = 0;

            Card? drewCard = null;

            for (int i = 0; i < drawAmount; i++)
            {
                Card? drawnCard = gameDeck.DealCard();

                if (drawnCard is null)
                    break;

                int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCards);

                if (awardedPenalty > 0)
                {
                    awardedPenaltyCount = awardedPenalty;

                    penaltyCardSpotted = true;

                    drewCard = drawnCard;

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

            if (!penaltyCardSpotted)
                return null;

            if (unfortunateSoul is Computer)
                return $" During the draw, {unfortunateSoul.Name} picked up the {drewCard}. Along with the normal draw, they will receive {awardedPenaltyCount} additional card(s).";
            else
                return $" During the draw, you picked up the {drewCard}. Along with the normal draw, you will receive {awardedPenaltyCount} additional card(s).";
        }
    }
}

/*
 MESSAGES:
    Trace played: DrawFour, so Sally had to draw! During the draw, Sally picked up the Ace of Spades. Along with the normal draw, they will recieve 4 additional card(s).
    You played: DrawFour, so Trace had to draw! During the draw, Trace picked up the DrawTwo. Along with the normal draw, they will recieve 0 additional card(s).
    Sally played: DrawFour, so Viper had to draw! During the draw, Viper picked up the Two of Hearts. Along with the normal draw, they will recieve 0 additional card(s).
    Trace decided to pick up and found the Ace of Spades! They picked up 2 additional cards!
    Trace played: DrawTwo, so Sally had to draw! During the draw, Sally picked up the Ten of Hearts. Along with the normal draw, they will recieve 0 additional card(s).
    Trace played: DrawFour, so Sally had to draw! During the draw, Sally picked up the Eight of Diamonds. Along with the normal draw, they will recieve 0 additional card(s).
    You played: DrawFour, so Trace had to draw! During the draw, Trace picked up the Two of Hearts. Along with the normal draw, they will recieve 0 additional card(s).
    Sally played: DrawFour, so you had to draw! During the draw, Human picked up the Queen of Spades. Along with the normal draw, you will recieve 0 additional card(s).
    You played: DrawTwo, so Trace had to draw! During the draw, Trace picked up the Queen of Spades. Along with the normal draw, they will recieve 0 additional card(s).

 */