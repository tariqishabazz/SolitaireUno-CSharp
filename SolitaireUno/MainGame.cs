/*
 MainGame.cs

 Purpose:
 - Orchestrates the overall game: maintains players, the game deck, turn handlers, and implements the main turn-advancing logic.
 - Responsible for game initialization (dealing hands, preparing the table) and processing each turn for both human and computer players.

 Commenting guideline applied:
 - File-level purpose header added to match the project's documentation style used in Home.razor.cs.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Main game orchestrator that manages players, deck and turn handlers.
    /// </summary>
    public class MainGame
    {
        public List<Player> AllPlayers { get; private set; } = [];

        public List<string> RandomComputerNames { get; } = ["Trace", "Sally", "Viper"];

        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public GameSettings CurrentGameSettings { get; private set; }

        public int CurrentTurnIndex { get; private set; }

        public Card? LastPlayedCard { get; private set; }
        public Card? LogicCard;
        public Card? VisualCard;

        internal RegularCard PenaltyCard { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainGame"/> class.
        /// </summary>
        /// <param name="deck">The deck to use for gameplay.</param>
        /// <param name="gameModeChoice">The game mode (ascending or descending).</param>
        /// <param name="suitEnforcement">Whether suit enforcement is enabled.</param>
        /// <param name="gameDifficulty">Difficulty level for computer AI.</param>
        /// <param name="numberOfPlayers">The number of players in game</param>
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


            // -------- SETTING PENALTY CARD, DECK ------- //

            PenaltyCard = new RegularCard(Suits.Spades, Values.Queen);

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
            int startingHandSize = 21 / AllPlayers.Count; // dividing from 21 so the deck isn't immediately depleted

            foreach (Player player in AllPlayers)
            {
                for (int i = 0; i < startingHandSize; i++)
                {
                    player.PickupCard(GameDeck.DealCard()!);
                }
            }

            LogicCard = GameDeck.PreventInitialSpecialCard();

            if (LogicCard is null)
                return;

            GameDeck.AddToDiscardPile(LogicCard);
            VisualCard = LogicCard;

            // resetting this bool to false at the start of the game,
            // so that if the deck was reshuffled during a previous game, it will be reset for the new game
            GameDeck.DeckReshuffled = false;
        }

        /// <summary>
        /// Advances the game by one turn, handling either the player or computer move.
        /// </summary>
        /// <param name="playerDecision">Optional player input or command used during the player's turn.</param>
        /// <returns>A UI message produced during the processed turn.</returns>
        public (string message, bool validDecision) AdvanceTurn(string playerDecision = "")
        {
            bool turnSkipped = false;                               // bool for if a player was skipped 
            string uiMessage;                        // initial empty string for message to be sent back
            int stepsToMove = 1;

            Player currentPlayer = AllPlayers[CurrentTurnIndex];    // holds the current player at time of turn

            if (LogicCard is null || VisualCard is null)
                return (string.Empty, false);


            // ---------------- A COMPUTER'S TURN ----------------- //

            if (currentPlayer is Computer computerPlayer)
            {
                int nextPlayersIndex = (CurrentTurnIndex + 1) % AllPlayers.Count;

                var (message, cardPlayed, successfulDecision) = _computerTurnHandler.HandleTurn(computerPlayer, ref LogicCard, ref VisualCard, PenaltyCard, AllPlayers[nextPlayersIndex], CurrentGameSettings);

                uiMessage = message;

                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;

                    var (potentialDrawMessage, targetSkipped) = GameMethods.ApplySpecialCardEffect(LastPlayedCard, turnSkipped, GameDeck, AllPlayers[nextPlayersIndex], PenaltyCard);

                    if (!string.IsNullOrEmpty(potentialDrawMessage))
                    {
                        uiMessage += $"<br/>{potentialDrawMessage}";
                    }

                    stepsToMove = targetSkipped ? 2 : 1;
                }
                //if a player was skipped, we move "2" steps or players
                CurrentTurnIndex = (CurrentTurnIndex + stepsToMove) % AllPlayers.Count;
                return (uiMessage, successfulDecision);
            }


            // --------------- HUMAN'S TURN -------------- // 

            else
            {
                int nextPlayersIndex = (CurrentTurnIndex + 1) % AllPlayers.Count;

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, playerDecision, AllPlayers[nextPlayersIndex], CurrentGameSettings);

                uiMessage = message;

                if (isSuccessful)
                {
                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;

                        var (potentialDrawMessage, targetSkipped) = GameMethods.ApplySpecialCardEffect(LastPlayedCard, turnSkipped, GameDeck, AllPlayers[nextPlayersIndex], PenaltyCard);

                        if (!string.IsNullOrEmpty(potentialDrawMessage))
                        {
                            uiMessage += $"<br/>{potentialDrawMessage}";
                        }

                        stepsToMove = targetSkipped ? 2 : 1;
                    }

                    CurrentTurnIndex = (CurrentTurnIndex + stepsToMove) % AllPlayers.Count;
                }

                return (uiMessage, isSuccessful);
            }
        }
    }
}

// Modulus Help: See how many times right value can go into left value
// multiply the right value times the amount it can go into initial left value
// subtract the product from the initial left number, answer is remainder