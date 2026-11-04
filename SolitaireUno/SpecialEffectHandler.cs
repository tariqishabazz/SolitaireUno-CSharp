namespace SolitaireUno
{
    /// <summary>
    /// Handles special card effects: skip, draw, reverse logic.
    /// </summary>
    public static class SpecialEffectHandler
    {
        /// <summary>
        /// Returns the action instruction associated with a special card; regular cards return DoNothing.
        /// </summary>
        /// <param name="currentCard">The card being evaluated for a special action.</param>
        /// <returns>The action instruction that should be applied based on the card type.</returns>
        public static ActionInstruction GetCardAction(Card? currentCard) => currentCard switch
        {
            SpecialCard { CardType: TypeOfSpecialCard.Skip } => ActionInstruction.SkipTurn,
            SpecialCard { CardType: TypeOfSpecialCard.DrawTwo } => ActionInstruction.DrawTwo,
            SpecialCard { CardType: TypeOfSpecialCard.DrawFour } => ActionInstruction.DrawFour,
            SpecialCard { CardType: TypeOfSpecialCard.Reverse } => ActionInstruction.Reverse,
            _ => ActionInstruction.DoNothing
        };

        /// <summary>
        /// Applies special-card effects (skip/draw/reverse) to the target player and returns the result.
        /// </summary>
        /// <param name="lastPlayedCard">The last card that was played which may trigger an effect.</param>
        /// <param name="targetSkipped">A flag indicating whether the target player is already skipped.</param>
        /// <param name="gameDeck">The deck used to draw penalty cards from.</param>
        /// <param name="targetPlayer">The player who will receive any drawn cards or be skipped.</param>
        /// <param name="currentGameState">The current game state containing penalty cards.</param>
        /// <param name="playerCount">The total number of players (used for reverse logic in 2-player games).</param>
        /// <returns>A tuple containing an optional message, whether target was skipped, and whether direction was reversed.</returns>
        public static (string? message, bool targetSkipped, bool directionReversed) ApplyEffect(Card? lastPlayedCard, bool targetSkipped, Deck gameDeck, Player targetPlayer, int playerCount, GameState currentGameState)
        {
            if (lastPlayedCard is null)
                return (null, targetSkipped, false);

            return GetCardAction(lastPlayedCard) switch
            {
                ActionInstruction.SkipTurn => (null, true, false),
                ActionInstruction.DrawTwo => (GameMethods.ProcessDraw(2, targetPlayer, gameDeck, currentGameState), true, false),
                ActionInstruction.DrawFour => (GameMethods.ProcessDraw(4, targetPlayer, gameDeck, currentGameState), true, false),
                ActionInstruction.Reverse => playerCount == 2 ? (null, true, false) : (null, false, true),
                _ => (null, targetSkipped, false)
            };
        }
    }
}