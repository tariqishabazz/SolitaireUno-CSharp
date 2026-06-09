// MainGame.cs — orchestrates game flow, players, deck and turn advancement.

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
        public int ConsecutivePasses { get; private set; } = 0;

        // if stalemated, players can continue sequence through skipping values to break stalemate
        public bool LeapFrogMode { get; private set; } = false;

        public Card? LastPlayedCard { get; private set; }
        public Card? LogicCard;
        public Card? VisualCard;

        internal List<RegularCard> PenaltyCards { get; private set; }

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
            int maxNumberOfCardsToDeal = 21;

            int startingHandSize = maxNumberOfCardsToDeal / AllPlayers.Count; // dividing from maxNumber so the deck isn't immediately depleted

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

            Player currentPlayer = AllPlayers[CurrentTurnIndex];    // holds the current player at time of turn

            if (LogicCard is null || VisualCard is null)
                return (string.Empty, false);


            // ---------------- A COMPUTER'S TURN ----------------- //

            if (currentPlayer is Computer computerPlayer)
            {
                int nextPlayersIndex = (CurrentTurnIndex + 1) % AllPlayers.Count;

                var (message, cardPlayed, successfulDecision) = _computerTurnHandler.HandleTurn(computerPlayer, ref LogicCard, ref VisualCard, PenaltyCards, AllPlayers[nextPlayersIndex], CurrentGameSettings, LeapFrogMode);
                uiMessage = message;

                // checking to see if players consecutively pass
                StalemateMonitor(cardPlayed);


                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;

                    var (potentialDrawMessage, targetSkipped) = GameMethods.ApplySpecialCardEffect(LastPlayedCard, turnSkipped, GameDeck, AllPlayers[nextPlayersIndex], PenaltyCards);

                    if (!string.IsNullOrEmpty(potentialDrawMessage))
                    {
                        uiMessage += $"{potentialDrawMessage}";
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

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCards, playerDecision, AllPlayers[nextPlayersIndex], CurrentGameSettings, LeapFrogMode);
                uiMessage = message;

                if (isSuccessful)
                {
                    // checking to see if players consecutively pass
                    StalemateMonitor(cardPlayed);

                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;

                        var (potentialDrawMessage, targetSkipped) = GameMethods.ApplySpecialCardEffect(LastPlayedCard, turnSkipped, GameDeck, AllPlayers[nextPlayersIndex], PenaltyCards);

                        if (!string.IsNullOrEmpty(potentialDrawMessage))
                        {
                            uiMessage += $"{potentialDrawMessage}";
                        }

                        stepsToMove = targetSkipped ? 2 : 1;
                    }

                    CurrentTurnIndex = (CurrentTurnIndex + stepsToMove) % AllPlayers.Count;
                }

                return (uiMessage, isSuccessful);
            }
        }

        /// <summary>
        /// Verifies whether all players passed a round, indicating a stalemate.
        /// </summary>
        /// <remarks>It turns on Leapfrog mode if so.</remarks>
        /// <param name="cardPlayed">The card a player MIGHT have played</param>
        private void StalemateMonitor(Card? cardPlayed)
        {
            bool isDeckDead = GameDeck.Length() == 0
                              && GameDeck.ReshuffleCount >= (CurrentGameSettings.Mode == GameMode.Both ? 3 : 1);

            ConsecutivePasses = (cardPlayed is null && isDeckDead) ? ConsecutivePasses + 1 : 0;

            LeapFrogMode = ConsecutivePasses >= AllPlayers.Count;
        }
    }
}

// Modulus Help: See how many times right value can go into left value
// multiply the right value times the amount it can go into initial left value
// subtract the product from the initial left number, answer is remainder