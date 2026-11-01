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
        public string[] RandomComputerNames { get; } = [ "Trace", "Sally", "Viper" ];
        internal List<RegularCard> PenaltyCards { get; private set; }

        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;
        private readonly GameRules _gameRules;

        public GameSettings CurrentGameSettings { get; private set; }
        public GameState? CurrentState { get; private set; } = null;

        /// <summary>
        /// Initializes a new instance of <see cref="MainGame"/> with the provided deck and game settings.
        /// </summary>
        /// <param name="deck">The deck to use for gameplay.</param>
        /// <param name="currentGameSettings">The game settings (mode, difficulty, suit enforcement, player count).</param>
        public MainGame(Deck deck, GameSettings currentGameSettings)
        {
            CurrentGameSettings = currentGameSettings;
            _gameRules = new GameRules(currentGameSettings);

            Player humanPlayer = new() { Name = "Human" };
            AllPlayers.Add(humanPlayer);

            // Add computer players
            for (int i = 0; i < currentGameSettings.NumberOfPlayers; i++)
            {
                Computer computerPlayer = new() { Name = $"{RandomComputerNames[ i ]}" };
                AllPlayers.Add(computerPlayer);
            }

            // Initialize penalty cards and game deck
            PenaltyCards = [ new(Suits.Spades, Values.Ace), new(Suits.Spades, Values.Queen) ];

            GameDeck = deck;

            // Initialize turn handlers
            _playerTurnHandler = new PlayerTurnHandler(humanPlayer, GameDeck);
            _computerTurnHandler = new ComputerTurnHandler(GameDeck, CurrentGameSettings.Difficulty);
        }

        /// <summary>
        /// Starts the game by preparing the initial table card and setting the starting player turn.
        /// </summary>
        public void StartGame()
        {
            Card initialCard = new GameInitialization(GameDeck).PreventInitialSpecialCard()!;

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
            int startingHandSize = maxNumberOfCardsToDeal / AllPlayers.Count;

            foreach (Player player in AllPlayers)
            {
                for (int i = 0; i < startingHandSize; i++)
                    player.PickupCard(GameDeck.DealCard()!);
            }

            if (CurrentState.LogicCard is null)
                return;

            GameDeck.AddToDiscardPile(CurrentState.LogicCard);

            // Reset reshuffle count for the new game
            GameDeck.ReshuffleCount = 0;
        }

        /// <summary>
        /// Advances the game by one turn, handling either the player or computer move.
        /// </summary>
        /// <param name="playerDecision">Optional player input or command used during the player's turn.</param>
        /// <returns>A UI message and bool validation produced during the processed turn.</returns>
        public (string message, bool validDecision) AdvanceTurn(string playerDecision = "")
        {
            string uiMessage;
            int stepsToMove = 1;

            Player currentPlayer = AllPlayers[ CurrentState!.CurrentTurnIndex ];

            if (CurrentState.LogicCard is null || CurrentState.VisualCard is null)
                return (string.Empty, false);

            // Process computer player's turn
            if (currentPlayer is Computer computerPlayer)
            {
                int computerHandAmountPreTurn = computerPlayer.Hand.Count;
                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (message, cardPlayed, successfulDecision) = _computerTurnHandler.HandleTurn(computerPlayer, PenaltyCards, AllPlayers[ nextPlayersIndex ], CurrentGameSettings, CurrentState);
                uiMessage = message;
                int computerHandAmountPostTurn = computerPlayer.Hand.Count;

                // Monitor for stalemate conditions
                _gameRules.CheckStalemateConditions(cardPlayed, computerHandAmountPreTurn, computerHandAmountPostTurn, GameDeck.Length(), GameDeck.ReshuffleCount, CurrentState);

                if (cardPlayed is not null)
                    (uiMessage, stepsToMove) = ProcessTurnResult(cardPlayed, nextPlayersIndex, uiMessage);

                // Advance to next player (skip +1 if previous player was skipped)
                CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                return (uiMessage, successfulDecision);
            }

            // Process human player's turn
            else
            {
                int playerHandAmountPreTurn = currentPlayer.Hand.Count;
                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(PenaltyCards, playerDecision, AllPlayers[ nextPlayersIndex ], CurrentGameSettings, CurrentState);
                uiMessage = message;
                int playerHandAmountPostTurn = currentPlayer.Hand.Count;

                if (isSuccessful)
                {
                    // Monitor for stalemate conditions
                    _gameRules.CheckStalemateConditions(cardPlayed, playerHandAmountPreTurn, playerHandAmountPostTurn, GameDeck.Length(), GameDeck.ReshuffleCount, CurrentState);

                    if (cardPlayed is not null)
                        (uiMessage, stepsToMove) = ProcessTurnResult(cardPlayed, nextPlayersIndex, uiMessage);

                    CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);
                }

                return (uiMessage, isSuccessful);
            }
        }

        /// <summary>
        /// Processes the result of a turn: applies special card effects and determines next move distance.
        /// </summary>
        /// <param name="cardPlayed">The card that was played.</param>
        /// <param name="nextPlayersIndex">Index of the next player.</param>
        /// <param name="currentMessage">The current UI message to append effects to.</param>
        /// <returns>Updated message and steps to move for the next turn advancement.</returns>
        private (string message, int stepsToMove) ProcessTurnResult(Card cardPlayed, int nextPlayersIndex, string currentMessage)
        {
            var (potentialPlayMessage, targetSkipped, isDirectionReversed) = SpecialEffectHandler.ApplyEffect(CurrentState!.VisualCard, false, GameDeck, AllPlayers[nextPlayersIndex], PenaltyCards, AllPlayers.Count);

            string updatedMessage = currentMessage;

            if (!string.IsNullOrEmpty(potentialPlayMessage))
                updatedMessage += $"{potentialPlayMessage}";

            if (isDirectionReversed)
                CurrentState.PlayDirection *= -1;

            int stepsToMove = targetSkipped ? 2 : 1;

            return (updatedMessage, stepsToMove);
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
