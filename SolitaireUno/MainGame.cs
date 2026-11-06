namespace SolitaireUno
{
    public class GameState
    {
        public List<Player> CurrentPlayers { get; set; } = [];

        public List<RegularCard> PenaltyCards { get; set; } = [];

        public Card? LogicCard { get; set; } = null;
        public Card? VisualCard { get; set; } = null;

        public int CurrentTurnIndex { get; set; }
        public int ConsecutivePasses { get; set; } = 0;
        public int ConsecutivePickups { get; set; } = 0;
        public int PlayDirection { get; set; } = 1;
        public int CurrentDeckSize { get; set; } = 0;
        public int DeckReshuffleCount { get; set; } = 0;
        public int DiscardPileCount { get; set; } = 0;

        public string UIMessage { get; set; } = string.Empty;

        public bool LeapFrogMode { get; set; } = false;
    }

    /// <summary>
    /// Main game orchestrator that manages players, deck and turn handlers.
    /// </summary>
    public class MainGame
    {
        public List<Player> AllPlayers { get; private set; } = [];

        private string[] RandomComputerNames { get; } = [
            "Kai", "Soren", "Zara", "Milan", "Nova",
            "Luca", "Aria", "Ezra", "Milo", "Lyra"
        ];

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

            Player humanPlayer = new() { Name = currentGameSettings.PlayerName };
            AllPlayers.Add(humanPlayer);

            // Add computer players
            while (AllPlayers.Count - 1 < currentGameSettings.NumberOfPlayers)
            {
                string randomOpponentName = $"{RandomComputerNames[ Random.Shared.Next(RandomComputerNames.Length) ]}";

                bool playerExists = AllPlayers.Any(player => player.Name.Equals(randomOpponentName, StringComparison.OrdinalIgnoreCase));

                if (!playerExists)
                {
                    Computer computerPlayer = new() { Name = randomOpponentName };
                    AllPlayers.Add(computerPlayer);
                }
            }

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
            Card? initialCard = new GameInitialization(GameDeck).PreventInitialSpecialCard() ??
                throw new InvalidOperationException("The game could not start because the initial card was null.");

            CurrentState = new GameState
            {
                CurrentPlayers = [ .. AllPlayers ],

                PenaltyCards = [
                    new(Suits.Spades, Values.Ace),
                    new(Suits.Spades, Values.Queen)
                ],

                LogicCard = initialCard,
                VisualCard = initialCard,

                CurrentTurnIndex = Random.Shared.Next(AllPlayers.Count),
                ConsecutivePasses = 0,
                PlayDirection = 1,
                UIMessage = "",
                ConsecutivePickups = 0,

                LeapFrogMode = false,

                CurrentDeckSize = GameDeck.Length(),
                DeckReshuffleCount = GameDeck.ReshuffleCount,
                DiscardPileCount = GameDeck.DiscardPile.Count,

            } ?? throw new InvalidOperationException("The game's state did not render correctly.");

            int maxNumberOfCardsToDeal = 20;
            int startingHandSize = maxNumberOfCardsToDeal / CurrentState.CurrentPlayers.Count;

            foreach (Player player in CurrentState.CurrentPlayers)
                for (int i = 0; i < startingHandSize; i++)
                    player.PickupCard(GameDeck.DealCard()!);

            if (CurrentState.LogicCard is not null)
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
            if (CurrentState is null)
                throw new InvalidOperationException("The game's state did not render correctly.");

            int stepsToMove = 1;

            Player currentPlayer = CurrentState.CurrentPlayers[ CurrentState.CurrentTurnIndex ];

            // Process computer player's turn
            if (currentPlayer is Computer computerPlayer)
            {
                int computerHandAmountPreTurn = computerPlayer.Hand.Count;

                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (message, cardPlayed, successfulDecision) = _computerTurnHandler.HandleTurn(computerPlayer, AllPlayers[ nextPlayersIndex ], CurrentGameSettings, CurrentState);
                CurrentState.UIMessage = message;

                int computerHandAmountPostTurn = computerPlayer.Hand.Count;

                // Monitor for stalemate conditions
                _gameRules.CheckStalemateConditions(cardPlayed, computerHandAmountPreTurn, computerHandAmountPostTurn, CurrentState);

                if (cardPlayed is not null)
                    (CurrentState.UIMessage, stepsToMove) = ProcessTurnResult(cardPlayed, nextPlayersIndex, CurrentState.UIMessage);

                // Advance to next player (skip +1 if previous player was skipped)
                CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                UpdateGameState(CurrentState);
                return (CurrentState.UIMessage, successfulDecision);
            }

            // Process human player's turn
            else
            {
                int playerHandAmountPreTurn = currentPlayer.Hand.Count;

                int nextPlayersIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(playerDecision, AllPlayers[ nextPlayersIndex ], CurrentGameSettings, CurrentState);
                CurrentState.UIMessage = message;

                int playerHandAmountPostTurn = currentPlayer.Hand.Count;

                if (isSuccessful)
                {
                    // Monitor for stalemate conditions
                    _gameRules.CheckStalemateConditions(cardPlayed, playerHandAmountPreTurn, playerHandAmountPostTurn, CurrentState);

                    if (cardPlayed is not null)
                        (CurrentState.UIMessage, stepsToMove) = ProcessTurnResult(cardPlayed, nextPlayersIndex, CurrentState.UIMessage);

                    CurrentState.CurrentTurnIndex = GetNextPlayerIndex(CurrentState.CurrentTurnIndex, stepsToMove, CurrentState.PlayDirection, AllPlayers.Count);
                }

                UpdateGameState(CurrentState);
                return (CurrentState.UIMessage, isSuccessful);
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
            if (CurrentState is null)
                throw new InvalidOperationException("The game's state did not render correctly.");

            var (potentialPlayMessage, targetSkipped, isDirectionReversed) = SpecialEffectHandler.ApplyEffect(cardPlayed, false, GameDeck, AllPlayers[ nextPlayersIndex ], AllPlayers.Count, CurrentState);

            string updatedMessage = currentMessage;

            if (!string.IsNullOrEmpty(potentialPlayMessage))
                updatedMessage += $"{potentialPlayMessage}";

            if (isDirectionReversed)
                CurrentState.PlayDirection *= -1;

            int stepsToMove = targetSkipped ? 2 : 1;

            return (updatedMessage, stepsToMove);
        }

        private void UpdateGameState(GameState currentGameState)
        {
            currentGameState.CurrentPlayers = currentGameState.CurrentPlayers;

            currentGameState.PenaltyCards = currentGameState.PenaltyCards;
            currentGameState.LogicCard = currentGameState.LogicCard;
            currentGameState.VisualCard = currentGameState.VisualCard;

            currentGameState.CurrentTurnIndex = currentGameState.CurrentTurnIndex;
            currentGameState.PlayDirection = currentGameState.PlayDirection;

            currentGameState.ConsecutivePasses = currentGameState.ConsecutivePasses;
            currentGameState.ConsecutivePickups = currentGameState.ConsecutivePickups;

            currentGameState.DeckReshuffleCount = GameDeck.ReshuffleCount;
            currentGameState.CurrentDeckSize = GameDeck.Length();
            currentGameState.DiscardPileCount = GameDeck.DiscardPile.Count;

            currentGameState.LeapFrogMode = currentGameState.LeapFrogMode;

            currentGameState.UIMessage = currentGameState.UIMessage;
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