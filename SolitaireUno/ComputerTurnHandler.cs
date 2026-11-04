// ComputerTurnHandler.cs — handles computer turn processing and pickup/penalty logic.

namespace SolitaireUno
{
    /// <summary>
    /// Handles the computer's turn and applies drawing/penalty logic.
    /// </summary>
    public class ComputerTurnHandler(Deck deck, GameDifficulty currentDifficulty)
    {
        private readonly Deck _deck = deck;
        private readonly GameDifficulty _gameDifficulty = currentDifficulty;

        /// <summary>
        /// Processes the computer's turn and returns a message and the card played if any.
        /// </summary>
        /// <param name="currentComputerPlayer">The computer player taking the turn.</param>
        /// <param name="logicCard">Reference to the logic card used for validation.</param>
        /// <param name="visualCard">Reference to the visual card shown to the UI.</param>
        /// <param name="penaltyCard">The configured penalty card.</param>
        /// <param name="nextPlayer">The player who will act next (used for messages and penalty application).</param>
        /// <param name="currentGameSettings">Current game settings (mode, difficulty, suit enforcement, player count).</param>
        /// <returns>Tuple containing a UI message, the card played (if any), and whether the move was successful.</returns>
        public (string message, Card? playedCard, bool successfulMove) HandleTurn(Computer currentComputerPlayer, Player nextPlayer, GameSettings currentGameSettings, GameState currentGameState)
        {
            Card? potentialComputerPlay = currentComputerPlayer.MakeMove(nextPlayer.Hand.Count, currentGameSettings, currentGameState);

            // Computer has no valid play available
            if (potentialComputerPlay is null)
            {
                int maxAllowedReshuffles = currentGameSettings.Mode is GameMode.Both ? 2 : 1;

                if (currentGameState.CurrentDeckSize > 0 || currentGameState.CurrentDeckSize == 0 && (_deck.ReshuffleCount < maxAllowedReshuffles))
                    return HandlePickup(currentComputerPlayer, currentGameState);

                // Computer cannot pick up, check for reverse discard option
                else
                {
                    // Check if discard pile has at least 2 cards to enable reverse
                    if (currentGameState.DiscardPileCount > 1)
                    {
                        Card? potentialDiscard = _deck.ReverseDiscard();

                        if (potentialDiscard is null)
                            return (message: $"{currentComputerPlayer.Name} couldn't reverse the discard, discard pile is empty!", playedCard: null, successfulMove: true);

                        // Peek at discard pile after pulling the last card
                        if (_deck.DiscardPile.TryPeek(out Card? potentialFutureVisualCard))
                        {
                            // Determine the logic card to validate against after pulling discard
                            Card potentialFutureLogicCard = _deck.DiscardPile.FirstOrDefault(card => card is RegularCard) ?? potentialFutureVisualCard;

                            // Check if computer can play a card in their hand against the future logical card
                            bool iCanPlayCardInHand = currentComputerPlayer.Hand.Any(card => CardValidation.ValidCard(card, currentGameSettings, currentGameState));

                            // If the computer can play, update the game state cards
                            if (iCanPlayCardInHand)
                            {
                                currentGameState.VisualCard = potentialFutureVisualCard;
                                currentGameState.LogicCard = potentialFutureLogicCard;

                                // Add the pulled discard to their hand
                                currentComputerPlayer.Hand.Add(potentialDiscard);

                                return (message: $"Reverse Reverse! {currentComputerPlayer.Name} pulled a card, showing the {currentGameState.LogicCard}", playedCard: null, successfulMove: true);
                            }
                        }

                        // Reverse strategy failed, restore the card to the discard pile
                        _deck.AddToDiscardPile(potentialDiscard);
                    }

                    return (message: $"{currentComputerPlayer.Name} decided to pass!", playedCard: null, successfulMove: true);
                }
            }

            // Computer has valid play available
            else
            {
                currentGameState.VisualCard = potentialComputerPlay;

                if (potentialComputerPlay is RegularCard)
                    currentGameState.LogicCard = potentialComputerPlay;

                currentComputerPlayer.PlayCard(potentialComputerPlay);
                _deck.AddToDiscardPile(potentialComputerPlay);

                TypeOfSpecialCard? specialCardType = potentialComputerPlay is SpecialCard specialCard ? specialCard.CardType : null;

                bool isSkipCard = specialCardType is TypeOfSpecialCard.Skip;
                bool isDrawTwoCard = specialCardType is TypeOfSpecialCard.DrawTwo;
                bool isDrawFourCard = specialCardType is TypeOfSpecialCard.DrawFour;

                if (isSkipCard)
                {
                    if (nextPlayer is not Computer)
                        return (message: $"{currentComputerPlayer.Name} played: {potentialComputerPlay} and skipped your turn!", playedCard: potentialComputerPlay, successfulMove: true);

                    return (message: $"{currentComputerPlayer.Name} played: {potentialComputerPlay} and skipped {nextPlayer.Name}!", playedCard: potentialComputerPlay, successfulMove: true);
                }
                else if (isDrawFourCard || isDrawTwoCard)
                {
                    if (nextPlayer is not Computer)
                        return (message: $"{currentComputerPlayer.Name} played: {potentialComputerPlay}, so you had to draw!", playedCard: potentialComputerPlay, successfulMove: true);

                    return (message: $"{currentComputerPlayer.Name} played: {potentialComputerPlay}, so {nextPlayer.Name} had to draw!", playedCard: potentialComputerPlay, successfulMove: true);
                }

                return (message: $"{currentComputerPlayer.Name} decided to play: {potentialComputerPlay}!", playedCard: potentialComputerPlay, successfulMove: true);
            }
        }

        /// <summary>
        /// Handles the logic for a computer player to pick up a card in non-both game modes, applying any penalty cards
        /// as required.
        /// </summary>
        /// <remarks>This method is intended for use in game modes where only one type of pickup is
        /// allowed. It determines if a penalty applies and ensures the correct number of cards are picked up by the
        /// computer player.</remarks>
        /// <param name="currentComputerPlayer">The computer player who is performing the pickup action.</param>
        /// <param name="penaltyCards">The penalty cards that may trigger additional cards to be picked up, depending on the game rules.</param>
        /// <returns>A tuple containing a message describing the action taken, the card played (always null in this context), and
        /// a value indicating whether the move was successful.</returns>
        private (string message, Card? playedCard, bool successfulMove) HandlePickup(Player currentComputerPlayer, GameState currentGameState)
        {
            var (drawnCard, penaltyCount, actualPenaltyCardsDealt) = PickupProcessor.ProcessPickup(_deck, currentComputerPlayer, currentGameState);

            if (drawnCard is null)
                return (message: $"{currentComputerPlayer.Name} couldn't pick up, deck is empty!", playedCard: null, successfulMove: true);

            return HandlePotentialPenalty(currentComputerPlayer, drawnCard, penaltyCount, actualPenaltyCardsDealt);
        }

        private (string message, Card? playedCard, bool successfulMove) HandlePotentialPenalty(Player currentComputerPlayer, Card card, int penaltyCount, int actualPenaltyCardsDealt)
        {
            // NO PENALTY, NORMAL PICKUP
            if (penaltyCount == 0)
                return (message: $"{currentComputerPlayer.Name} decided to pick up!", playedCard: null, successfulMove: true);

            return (message: $"{currentComputerPlayer.Name} decided to pick up and found the {card}! They picked up {actualPenaltyCardsDealt} additional card(s)!", playedCard: null, successfulMove: true);
        }
    }
}