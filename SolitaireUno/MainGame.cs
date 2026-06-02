using System.Text;

namespace SolitaireUno
{
    /// <summary>
    /// Main game orchestrator that manages players, deck and turn handlers.
    /// </summary>
    public class MainGame
    {
        public List<Player> AllPlayers { get; private set; } = new List<Player>();
        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public GameMode GameModeChoice { get; set; }
        internal GameDifficulty GameDifficulty { get; set; }

        public int NumberOfPlayers { get; private set; }
        public int CurrentTurnIndex { get; private set; }
        internal bool SuitEnforcement { get; private set; }

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
            NumberOfPlayers = currentGameSettings.NumberOfPlayers;

            Player humanPlayer = new Player()
            {
                Name = "Human"
            };

            AllPlayers.Add(humanPlayer);


            // ------------ ADDING COMPUTER PLAYERS ------------ //

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                Computer computerPlayer = new Computer()
                {
                    Name = $"Computer {i + 1}"
                };

                AllPlayers.Add(computerPlayer);
            }


            // -------- SETTING PENALTY CARD, DECK, AND OTHER CONFIGURATIONS ------- //

            PenaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            GameDeck = deck;
            GameModeChoice = currentGameSettings.Mode;
            GameDifficulty = currentGameSettings.Difficulty;
            SuitEnforcement = currentGameSettings.SuitsEnforced;


            // --------- SETTING TURN HANDLERS --------- //

            _playerTurnHandler = new PlayerTurnHandler(humanPlayer, GameDeck);
            _computerTurnHandler = new ComputerTurnHandler(GameDeck, GameDifficulty);

            // resets bool if game started again
            deck.DeckReshuffled = false;
        }

        /// <summary>
        /// Starts the game by preparing the initial table card and setting the starting player turn.
        /// </summary>
        public void StartGame()
        {
            int startingHandSize = 21 / AllPlayers.Count;

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
        }

        /// <summary>
        /// Advances the game by one turn, handling either the player or computer move.
        /// </summary>
        /// <param name="playerDecision">Optional player input or command used during the player's turn.</param>
        /// <returns>A UI message produced during the processed turn.</returns>
        public string AdvanceTurn(string playerDecision = "")
        {
            bool turnSkipped = false;                               // bool for if a player was skipped 
            string uiMessage = string.Empty;                        // initial empty string for message to be sent back
            Player currentPlayer = AllPlayers[CurrentTurnIndex];    // holds the current player at time of turn

            if (LogicCard is null || VisualCard is null)
                return string.Empty;


            // ---------------- A COMPUTER'S TURN ----------------- //

            if (currentPlayer is Computer computerPlayer)
            {
                int nextPlayersIndex = (CurrentTurnIndex + 1) % AllPlayers.Count;

                var (message, cardPlayed) = _computerTurnHandler.HandleTurn(computerPlayer,
                                                                            ref LogicCard,
                                                                            ref VisualCard,
                                                                            PenaltyCard,
                                                                            AllPlayers[nextPlayersIndex],
                                                                            
                                                                            );

                uiMessage = message;

                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;
                    
                    turnSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard,
                                                                     turnSkipped,
                                                                     GameDeck,
                                                                     AllPlayers[nextPlayersIndex],
                                                                     PenaltyCard);

                }
            }


            // --------------- HUMAN'S TURN -------------- // 

            else
            {
                int nextPlayersIndex = (CurrentTurnIndex + 1) % AllPlayers.Count;

                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard,
                                                                                        ref VisualCard,
                                                                                        PenaltyCard,
                                                                                        playerDecision,
                                                                                        AllPlayers[nextPlayersIndex],
                                                                                        GameModeChoice,
                                                                                        SuitEnforcement);

                uiMessage = message;

                if (isSuccessful)
                {
                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        
                        turnSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard,
                                                                                   turnSkipped,
                                                                                   GameDeck,
                                                                                   AllPlayers[nextPlayersIndex],
                                                                                   PenaltyCard);
                    }
                }
            }

            // if a player was skipped, we move "2" steps or players
            int stepsToMove = turnSkipped ? 2 : 1; 
            CurrentTurnIndex = (CurrentTurnIndex + stepsToMove) % AllPlayers.Count; // allows for circular looping of turns

            return uiMessage;
        }
    }
}

// Modulus Help: See how many times right value can go into left value
    // multiply the right value times the amount it can go into initial left value
    // subtract the product from the initial left number, answer is remainder