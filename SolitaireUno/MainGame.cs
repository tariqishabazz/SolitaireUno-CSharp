using System.Text;

namespace SolitaireUno
{
    /// <summary>
    /// Main game orchestrator that manages players, deck and turn handlers.
    /// </summary>
    public class MainGame
    {
        public Player Player { get; private set; }
        public List<Computer> ComputerPlayers { get; private set; } = new List<Computer>();
        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public GameMode GameModeChoice { get; set; }
        internal GameDifficulty GameDifficulty { get; set; }

        public int NumberOfPlayers { get; private set; }
        public int CurrentTurnIndex { get; private set; }
        internal bool SuitEnforcement { get; private set; }
        public bool ComputerSkipped { get; set; }
        public bool PlayerSkipped { get; set; }

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
        public MainGame(Deck deck, GameMode gameModeChoice, bool suitEnforcement, GameDifficulty gameDifficulty, int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;

            Player = new Player(deck);

            for (int i = 0; i < numberOfPlayers; i++)
            {
                Computer computerPlayer = new Computer(deck);
                ComputerPlayers.Add(computerPlayer);
            }

            PenaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            GameDeck = deck;
            GameModeChoice = gameModeChoice;
            GameDifficulty = gameDifficulty;
            SuitEnforcement = suitEnforcement;

            _playerTurnHandler = new PlayerTurnHandler(Player, GameDeck);

            _computerTurnHandler = new ComputerTurnHandler(Computer, GameDeck, GameDifficulty);

            // resets bool if game started again
            deck.DeckReshuffled = false;
        }

        /// <summary>
        /// Starts the game by preparing the initial table card and setting the starting player turn.
        /// </summary>
        public void StartGame()
        {
            LogicCard = GameDeck.PreventInitialSpecialCard();

            if (LogicCard is null)
                return;

            GameDeck.AddToDiscardPile(LogicCard);
            VisualCard = LogicCard;

            CurrentTurnIndex = (CurrentTurnIndex + 1) % NumberOfPlayers;
        }

        /// <summary>
        /// Advances the game by one turn, handling either the player or computer move.
        /// </summary>
        /// <param name="playerDecision">Optional player input or command used during the player's turn.</param>
        /// <returns>A UI message produced during the processed turn.</returns>
        public string AdvanceTurn(string playerDecision = "")
        {
            PlayerSkipped = false;
            ComputerSkipped = false;

            string uiMessage = "";

            // --------------- PLAYER'S TURN -------------- // 

            if (CurrentTurnIndex == 0 && (LogicCard is not null && VisualCard is not null))
            {
                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, playerDecision, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (isSuccessful)
                {
                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        ComputerSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard, 
                                                                             ComputerSkipped, 
                                                                             GameDeck, 
                                                                             ComputerPlayers[CurrentTurnIndex - 1], 
                                                                             PenaltyCard);
                    }

                    if (!ComputerSkipped)
                        CurrentTurnIndex++;
                }
            }

            // ---------------- A COMPUTER'S TURN ----------------- //

            else if (CurrentTurnIndex > 0 && (LogicCard is not null && VisualCard is not null))
            {
                var (message, cardPlayed) = _computerTurnHandler.HandleTurn(ComputerPlayers[CurrentTurnIndex - 1], ref LogicCard, ref VisualCard, PenaltyCard, Player.Hand.Count, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;
                    PlayerSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard,
                                                                       PlayerSkipped,
                                                                       GameDeck,
                                                                       ComputerPlayers[CurrentTurnIndex = (CurrentTurnIndex + 1) % NumberOfPlayers],
                                                                       PenaltyCard);
                }

                if (!PlayerSkipped)
                    CurrentTurnIndex = (CurrentTurnIndex + 1) % NumberOfPlayers;

            }

            return uiMessage;
        }
    }
}