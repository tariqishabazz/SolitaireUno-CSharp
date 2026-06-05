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
        /// <param name="logicCard">Reference to the logic card used for validation.</param>
        /// <param name="visualCard">Reference to the visual card shown to the UI.</param>
        /// <param name="penaltyCard">The configured penalty card.</param>
        /// <param name="opponentHandSize">Number of cards the opponent currently holds.</param>
        /// <param name="currentGameMode">Current game mode for validation.</param>
        /// <param name="suitEnforcement">Whether suit enforcement is active.</param>
        /// <returns>Tuple containing a UI message and the card played, if any.</returns>
        public (string message, Card? playedCard, bool successfulMove) HandleTurn(Computer currentComputerPlayer, ref Card logicCard, ref Card visualCard, Card penaltyCard, Player nextPlayer, GameSettings currentGameSettings)
        {
            Card? potentialComputerPlay = currentComputerPlayer.MakeMove(logicCard, nextPlayer.Hand.Count, _deck.Length(), currentGameSettings);


            // ----------------- IF COMPUTER HAS NO POTENTIAL PLAY ------------------ //

            if (potentialComputerPlay is null)
            {
                if (_deck.Length() > 0 || (_deck.Length() == 0 && !_deck.DeckReshuffled && currentGameSettings.Mode is not GameMode.Both))
                {
                    Card card = _deck.DealCard()!;
                    currentComputerPlayer.PickupCard(card);


                    // ------------- HANDLES POTENTIAL PENALTY --------------- //

                    int computerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCard);
                    if (computerPotentialPenaltyCount > 0)
                    {
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
                        return ($"{currentComputerPlayer.Name} decided to pick up and found the {penaltyCard}! They picked up {actualPickupCount} additional cards!", null, true);
                    }

                    else
                        return ($"{currentComputerPlayer.Name} decided to pick up!", null, true);
                }

                else if (_deck.Length() == 0 && _deck.DeckReshuffled)
                    return ($"{currentComputerPlayer.Name} decided to pass!", null, true);
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
                bool isDrawTwo = potentialComputerPlay is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawTwo;
                bool isDrawFour = potentialComputerPlay is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawFour;

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
                
                else if (isDrawFour || isDrawTwo)
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

            return ($"{currentComputerPlayer.Name} got scared...", null, false);
        }

    }
}
