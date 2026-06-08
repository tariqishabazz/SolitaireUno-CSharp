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
        public (string message, Card? playedCard, bool successfulMove) HandleTurn(Computer currentComputerPlayer, ref Card logicCard, ref Card visualCard, List<RegularCard> penaltyCards, Player nextPlayer, GameSettings currentGameSettings, bool isLeapFrog)
        {
            Card? potentialComputerPlay = currentComputerPlayer.MakeMove(logicCard, nextPlayer.Hand.Count, _deck.Length(), currentGameSettings, isLeapFrog);


            // ----------------- IF COMPUTER HAS NO POTENTIAL PLAY ------------------ //

            if (potentialComputerPlay is null)
            {
                int maxAllowedReshuffles = currentGameSettings.Mode is GameMode.Both ? 3 : 1;

                if (_deck.Length() > 0 || _deck.Length() == 0 && (_deck.ReshuffleCount < maxAllowedReshuffles))
                {
                    return HandlePickup(currentComputerPlayer, penaltyCards);
                }

                else
                {
                    return ($"{currentComputerPlayer.Name} decided to pass!", null, true);
                }
            }


            // ------------------ COMPUTER HAS VALID PLAY --------------- // 

            else
            {
                visualCard = potentialComputerPlay;

                if (potentialComputerPlay is RegularCard)
                    logicCard = potentialComputerPlay;

                currentComputerPlayer.PlayCard(potentialComputerPlay);
                _deck.AddToDiscardPile(potentialComputerPlay);


                bool isSkipCard = potentialComputerPlay is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip;
                bool isDrawTwoCard = potentialComputerPlay is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawTwo;
                bool isDrawFourCard = potentialComputerPlay is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawFour;

                if (isSkipCard)
                {
                    if (nextPlayer is not Computer)
                    {
                        return ($"{currentComputerPlayer.Name} played: {potentialComputerPlay} " +
                            $"and skipped your turn!", potentialComputerPlay, true);
                    }

                    return ($"{currentComputerPlayer.Name} played: {potentialComputerPlay} " +
                        $"and skipped {nextPlayer.Name}!", potentialComputerPlay, true);
                }

                else if (isDrawFourCard || isDrawTwoCard)
                {
                    if (nextPlayer is not Computer)
                    {
                        return ($"{currentComputerPlayer.Name} played: {potentialComputerPlay}, " +
                            $"so you had to draw!", potentialComputerPlay, true);
                    }

                    return ($"{currentComputerPlayer.Name} played: {potentialComputerPlay}, " +
                        $"so {nextPlayer.Name} had to draw!", potentialComputerPlay, true);
                }

                return ($"{currentComputerPlayer.Name} decided to play: {potentialComputerPlay}!", potentialComputerPlay, true);
            }
        }


        // ======================== PRIVATE METHODS FOR PICKUP LOGIC ========================= //


        /// <summary>
        /// Handles the logic for a computer player to pick up a card in non-both game modes, applying any penalty cards
        /// as required.
        /// </summary>
        /// <remarks>This method is intended for use in game modes where only one type of pickup is
        /// allowed. It determines if a penalty applies and ensures the correct number of cards are picked up by the
        /// computer player.</remarks>
        /// <param name="currentComputerPlayer">The computer player who is performing the pickup action.</param>
        /// <param name="penaltyCard">The penalty card that may trigger additional cards to be picked up, depending on the game rules.</param>
        /// <returns>A tuple containing a message describing the action taken, the card played (always null in this context), and
        /// a value indicating whether the move was successful.</returns>
        private (string message, Card? playedCard, bool successfulMove) HandlePickup(Player currentComputerPlayer, List<RegularCard> penaltyCards)
        {
            Card card = _deck.DealCard()!;
            currentComputerPlayer.PickupCard(card);

            // ------------- HANDLES POTENTIAL PENALTY --------------- //

            return HandlePotentialPenalty(currentComputerPlayer, card, penaltyCards);
        }


        private (string message, Card? playedCard, bool successfulMove) HandlePotentialPenalty(Player currentComputerPlayer, Card card, List<RegularCard> penaltyCards)
        {
            int computerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCards);

            if (computerPotentialPenaltyCount == 0) // No penalty, just a normal pickup
            {
                return ($"{currentComputerPlayer.Name} decided to pick up!", null, true);
            }

            int actualPickupCount = 0;

            for (int i = 0; i < computerPotentialPenaltyCount; i++)
            {
                Card? addtionalPenaltyCard = _deck.DealCard();

                if (addtionalPenaltyCard is not null)
                {
                    currentComputerPlayer.PickupCard(addtionalPenaltyCard);
                    actualPickupCount++;
                }
            }
            return ($"{currentComputerPlayer.Name} decided to pick up and found the {card}! They picked up {actualPickupCount} additional cards!", null, true);
        }
    }
}
