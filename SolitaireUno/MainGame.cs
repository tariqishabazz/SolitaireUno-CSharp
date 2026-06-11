// MainGame.cs — orchestrates game flow, players, deck and turn advancement.

namespace SolitaireUno
{
    public class GameState
    {
        public Card LogicCard = null!;
        public Card VisualCard = null!;

        public int CurrentTurnIndex;
        public int ConsecutivePasses = 0;
        public int ConsecutivePickups = 0;
        public int PlayDirection = 1;

        public bool LeapFrogMode = false;
    }

    /// <summary>
    /// Main game orchestrator that manages players, deck and turn handlers.
    /// </summary>
    public class MainGame
    {
        public List<Player> AllPlayers { get; private set; } = [];
        public string[] RandomComputerNames { get; } = ["Trace", "Sally", "Viper"];
        internal List<RegularCard> PenaltyCards { get; private set; }

        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public GameSettings CurrentGameSettings { get; private set; }
        public GameState CurrentState { get; private set; } = null!;

        /// <summary>
        /// Initializes a new instance of <see cref="MainGame"/> with the provided deck and game settings.
        /// </summary>
        /// <param name="deck">The deck to use for gameplay.</param>
        /// <param name="currentGameSettings">The game settings (mode, difficulty, suit enforcement, player count).</param>
        public MainGame(Deck deck, GameSettings currentGameSettings)
        {
            CurrentGameSettings = currentGameSettings;

            Player humanPlayer = new()
            {
                Name = "Human"
            };

            AllPlayers.Add(humanPlayer);

            // ------------ ADDING COMPUTER PLAYERS ------------ //

            for (int i = 0; i < currentGameSettings.NumberOfPlayers; i++)
            {
                Computer computerPlayer = new()
                {
                    Name = $"{RandomComputerNames[i]}"
                };

                AllPlayers.Add(computerPlayer);
            }

            // -------- SETTING PENALTY CARDS, DECK ------- //

            PenaltyCards = [new(Suits.Spades, Values.Ace), new(Suits.Spades, Values.Queen)];

            GameDeck = deck;

            // --------- SETTING TURN HANDLERS --------- //

            _playerTurnHandler = new PlayerTurnHandler(humanPlayer, GameDeck);
            _computerTurnHandler = new ComputerTurnHandler(GameDeck, CurrentGameSettings.Difficulty);
        }

        /// <summary>
        /// Starts the game by preparing the initial table card and setting the starting player turn.
        /// </summary>
        public void StartGame()
        {
            Card initialCard = GameDeck.PreventInitialSpecialCard()!;

            CurrentState = new GameState
            {
                LogicCard = initialCard,
                VisualCard = initialCard,

                CurrentTurnIndex = 0,
                ConsecutivePasses = 0,
                PlayDirection = 1,
                ConsecutivePickups = 0,

                LeapFrogMode = false,
            };

            int maxNumberOfCardsToDeal = 21;
            int startingHandSize = maxNumberOfCardsToDeal / AllPlayers.Count; // dividing from maxNumber so the deck isn't immediately depleted

            foreach (Player player in AllPlayers)
            {
                for (int i = 0; i < startingHandSize; i++)
                    player.PickupCard(GameDeck.DealCard()!);
            }

            if (CurrentState.LogicCard is null)
                return;

            GameDeck.AddToDiscardPile(CurrentState.LogicCard);

            // resetting this for each new game
            GameDeck.ReshuffleCount = 0;
        }

        /// <summary>
        /// Advances the game by one turn, handling either the player or computer move.
        /// </summary>
        /// <param name="playerDecision">Optional player input or command used during the player's turn.</param>
        /// <returns>A UI message and bool validation produced during the processed turn.</returns>
        public (string message, bool validDecision) AdvanceTurn(string playerDecision = "")
        {
            bool turnSkipped = false;                               // bool for if a player was skipped
            string uiMessage;                        // initial empty string for message to be sent back
            int stepsToMove = 1;

            Player currentPlayer = AllPlayers[CurrentState.CurrentTurnIndex];    // holds the current player at time of turn

            if (CurrentState.LogicCard is null || CurrentState.VisualCard is null) // RETURN EARLY IF ANY CARD IS NULL
                return (string.Empty, false);

            // ===================== A COMPUTER'S TURN ===================== //

            if (currentPlayer is Computer computerPlayer)
            {
                int computerHandAmountPreTurn = computerPlayer.Hand.Count;

                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (message, cardPlayed, successfulDecision) = _computerTurnHandler.HandleTurn(computerPlayer,
                                                                                                PenaltyCards,
                                                                                                AllPlayers[nextPlayersIndex],
                                                                                                CurrentGameSettings,
                                                                                                CurrentState);
                uiMessage = message;

                int computerHandAmountPostTurn = computerPlayer.Hand.Count;

                // checking to see if players consecutively pass
                StalemateMonitor(cardPlayed, computerHandAmountPreTurn, computerHandAmountPostTurn);

                if (cardPlayed is not null)
                {
                    var (potentialPlayMessage, targetSkipped, isDirectionReversed) = GameMethods.ApplySpecialCardEffect(CurrentState.VisualCard,
                                                                                                                        turnSkipped,
                                                                                                                        GameDeck,
                                                                                                                        AllPlayers[nextPlayersIndex],
                                                                                                                        PenaltyCards,
                                                                                                                        AllPlayers.Count);

                    if (!string.IsNullOrEmpty(potentialPlayMessage))
                    {
                        uiMessage += $"{potentialPlayMessage}";
                    }

                    if (isDirectionReversed)
                        CurrentState.PlayDirection *= -1;

                    stepsToMove = targetSkipped ? 2 : 1;
                }

                //if a player was skipped, we move "2" steps or players
                CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                return (uiMessage, successfulDecision);
            }

            // ===================== HUMAN'S TURN ===================== //
            else
            {
                int playerHandAmountPreTurn = currentPlayer.Hand.Count;

                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(PenaltyCards,
                                                                                        playerDecision,
                                                                                        AllPlayers[nextPlayersIndex],
                                                                                        CurrentGameSettings,
                                                                                        CurrentState);
                uiMessage = message;

                int playerHandAmountPostTurn = currentPlayer.Hand.Count;

                if (isSuccessful)
                {
                    // checking to see if players consecutively pass
                    StalemateMonitor(cardPlayed, playerHandAmountPreTurn, playerHandAmountPostTurn);

                    if (cardPlayed is not null)
                    {
                        var (potentialPlayMessage, targetSkipped, isDirectionReversed) = GameMethods.ApplySpecialCardEffect(CurrentState.VisualCard,
                                                                                                                            turnSkipped,
                                                                                                                            GameDeck,
                                                                                                                            AllPlayers[nextPlayersIndex],
                                                                                                                            PenaltyCards,
                                                                                                                            AllPlayers.Count);

                        if (!string.IsNullOrEmpty(potentialPlayMessage))
                            uiMessage += $"{potentialPlayMessage}";

                        if (isDirectionReversed)
                            CurrentState.PlayDirection *= -1;

                        stepsToMove = targetSkipped ? 2 : 1;
                    }

                    CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);
                }

                return (uiMessage, isSuccessful);
            }
        }

        /// <summary>
        /// Verifies whether all players passed a round, indicating a stalemate.
        /// </summary>
        /// <remarks>It turns on Leapfrog mode if so.</remarks>
        /// <param name="cardPlayed">The card a player MIGHT have played</param>
        private void StalemateMonitor(Card? cardPlayed, int handAmountPreTurn, int handAmountPostTurn)
        {
            bool isDeckDead = GameDeck.Length() == 0
                              && GameDeck.ReshuffleCount >= (CurrentGameSettings.Mode == GameMode.Both ? 3 : 1);

            CurrentState.ConsecutivePasses = (cardPlayed is null && isDeckDead) ? CurrentState.ConsecutivePasses + 1 : 0;
            CurrentState.ConsecutivePickups = (cardPlayed is null && handAmountPreTurn < handAmountPostTurn) ? CurrentState.ConsecutivePickups + 1 : 0;

            // LEAPFROG WILL TURN ON IF...
            // 1) THE DECK IS EMPTY AND CAN'T BE RESHUFFLED AND ALL PLAYERS PASS OR...
            // 2) IF ALL PLAYERS REPEATEDLY PICKUP FOR 3 ROUNDS, HENCE * 3
            CurrentState.LeapFrogMode = CurrentState.ConsecutivePasses >= AllPlayers.Count || CurrentState.ConsecutivePickups >= (AllPlayers.Count * 3);
        }

        /// <summary>
        /// Retrieves the next player's index determined on the games current <paramref name="direction"/>
        /// </summary>
        /// <param name="currentIndex">Represents the current players index/turn</param>
        /// <param name="stepsToMove">The number of steps to increment</param>
        /// <param name="direction">The current direction of the game (1 for forwards, -1 for backwards/reversed) </param>
        /// <param name="totalPlayers">The number of players currently playing</param>
        /// <returns>The next players index as an int</returns>
        private static int GetNextPlayerIndex(int currentIndex, int stepsToMove, int direction, int totalPlayers)
        {
            int nextPlayersIndex = (currentIndex + (stepsToMove * direction)) % totalPlayers;

            return nextPlayersIndex < 0 ? nextPlayersIndex + totalPlayers : nextPlayersIndex;
        }
    }
}

// Modulus Help: See how many times right value can go into left value
// multiply the right value times the amount it can go into initial left value
// subtract the product from the initial left number, answer is remainder