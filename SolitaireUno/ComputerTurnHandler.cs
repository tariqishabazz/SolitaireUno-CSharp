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
        public (string message, Card? playedCard, bool successfulMove) HandleTurn(Computer currentComputerPlayer, List<RegularCard> penaltyCards, Player nextPlayer, GameSettings currentGameSettings, GameState currentGameState)
        {
            Card? potentialComputerPlay = currentComputerPlayer.MakeMove(currentGameState.LogicCard, nextPlayer.Hand.Count, _deck.Length(), currentGameSettings, currentGameState.LeapFrogMode);

            // ----------------- IF COMPUTER HAS NO POTENTIAL PLAY ------------------ //

            if (potentialComputerPlay is null)
            {
                int maxAllowedReshuffles = currentGameSettings.Mode is GameMode.Both ? 3 : 1;

                if (_deck.Length() > 0 || _deck.Length() == 0 && (_deck.ReshuffleCount < maxAllowedReshuffles))
                    return HandlePickup(currentComputerPlayer, penaltyCards);

                // =============== IF COMPUTER CAN'T PICKUP ============== //
                else
                {
                    // CHECK IF DISCARD PILE HAS AT LEAST 2 CARDS
                    if (_deck.DiscardPile.Count > 1)
                    {
                        Card potentialDiscard = _deck.ReverseDiscard()!;

                        // PEEK AT DISCARD PILE AFTER MOVING LAST DISCARD
                        if (_deck.DiscardPile.TryPeek(out Card? potentialFutureVisualCard))
                        {
                            // STORE THE POTENTIAL FUTURE LOGIC CARD IF KEEPING PULLED DISCARD
                            Card potentialFutureLogicCard = _deck.DiscardPile.FirstOrDefault(card => card is RegularCard) ?? potentialFutureVisualCard;

                            // THIS BOOL REPRESENTS WHETHER THE COMPUTER CAN
                            // 1) PLAY ANOTHER CARD CURRENTLY IN THEIR HAND AGAINST THE FUTURE LOGICAL CARD
                            bool iCanPlayCardInHand = currentComputerPlayer.Hand.Any(card => CardValidation.ValidCard(card, potentialFutureLogicCard, currentGameSettings, currentGameState.LeapFrogMode));

                            // IF THE COMPUTER CAN PLAY, UPDATE THE GAME CARDS
                            if (iCanPlayCardInHand)
                            {
                                currentGameState.VisualCard = potentialFutureVisualCard;
                                currentGameState.LogicCard = potentialFutureLogicCard;

                                // ADD THE PULLED DISCARD TO THEIR HAND
                                currentComputerPlayer.Hand.Add(potentialDiscard);

                                // RETURN MESSAGE
                                return (message: $"Reverse Reverse! {currentComputerPlayer.Name} pulled a card, showing the {currentGameState.LogicCard}", playedCard: null, successfulMove: true);
                            }
                        }

                        // IF WE HAVEN'T RETURNED AT THIS POINT,
                        // THE COMPUTER'S STRATEGY DIDN'T WORK, SO STORE THE CARD BACK IN THE PILE
                        _deck.AddToDiscardPile(potentialDiscard);
                    }

                    return (message: $"{currentComputerPlayer.Name} decided to pass!", playedCard: null, successfulMove: true);
                }
            }

            // ===================== COMPUTER HAS VALID PLAY ===================== //
            else
            {
                currentGameState.VisualCard = potentialComputerPlay;

                if (potentialComputerPlay is RegularCard)
                    currentGameState.LogicCard = potentialComputerPlay;

                currentComputerPlayer.PlayCard(potentialComputerPlay);
                _deck.AddToDiscardPile(potentialComputerPlay);

                bool isSkipCard = potentialComputerPlay is SpecialCard specialCard && specialCard.CardType is SpecialCardType.Skip;
                bool isDrawTwoCard = potentialComputerPlay is SpecialCard specialCard2 && specialCard2.CardType is SpecialCardType.DrawTwo;
                bool isDrawFourCard = potentialComputerPlay is SpecialCard specialCard3 && specialCard3.CardType is SpecialCardType.DrawFour;

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

        // ======================== PRIVATE METHODS FOR PICKUP LOGIC ========================= //

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
        private (string message, Card? playedCard, bool successfulMove) HandlePickup(Player currentComputerPlayer, List<RegularCard> penaltyCards)
        {
            Card card = _deck.DealCard()!;
            currentComputerPlayer.PickupCard(card);

            // ------------- HANDLES POTENTIAL PENALTY --------------- //

            return HandlePotentialPenalty(currentComputerPlayer, card, penaltyCards);
        }

        /// <summary>
        /// Handles a computers pickup from deck and deals additional card if a penalty card was found.
        /// </summary>
        /// <param name="currentComputerPlayer"></param>
        /// <param name="card"></param>
        /// <param name="penaltyCards"></param>
        /// <returns></returns>
        private (string message, Card? playedCard, bool successfulMove) HandlePotentialPenalty(Player currentComputerPlayer, Card card, List<RegularCard> penaltyCards)
        {
            int computerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCards);

            // NO PENALTY, NORMAL PICKUP
            if (computerPotentialPenaltyCount == 0)
                return (message: $"{currentComputerPlayer.Name} decided to pick up!", playedCard: null, successfulMove: true);

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

            return (message: $"{currentComputerPlayer.Name} decided to pick up and found the {card}! They picked up {actualPickupCount} additional card(s)!", playedCard: null, successfulMove: true);
        }
    }
}