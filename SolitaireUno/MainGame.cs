using System.Text;

namespace SolitaireUno
{
    public class MainGame
    {
        public Player Player { get; private set; }
        public Computer Computer { get; private set; }
        public Deck GameDeck { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public GameMode GameModeChoice { get; set; }
        internal GameDifficulty GameDifficulty { get; set; }

        public bool IsPlayerTurn { get; set; }
        internal bool SuitEnforcement { get; private set; }
        public bool ComputerSkipped { get; set; }
        public bool PlayerSkipped { get; set; }

        public Card? LastPlayedCard { get; private set; }
        public Card? LogicCard;
        public Card? VisualCard;
        internal RegularCard PenaltyCard { get; private set; }

        /// <summary>
        /// Main constructor for a new game. It handles creating new player and computer objects, setting all the game modes/features, initializing the turn handlers,
        ///     and setting up each player's hand of cards
        /// </summary>
        /// <param name="deck">The game deck </param>
        /// <param name="gameModeChoice">The game mode chosen</param>
        /// <param name="suitEnforcement">Whether suits are enforced</param>
        /// <param name="gameDifficulty">The game's difficulty</param>
        public MainGame(Deck deck, GameMode gameModeChoice, bool suitEnforcement, GameDifficulty gameDifficulty)
        {
            Player = new Player(deck);
            Computer = new Computer(deck);
            PenaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            GameDeck = deck;
            GameModeChoice = gameModeChoice;
            GameDifficulty = gameDifficulty;
            SuitEnforcement = suitEnforcement;

            _playerTurnHandler = new PlayerTurnHandler(Player, GameDeck);
            _computerTurnHandler = new ComputerTurnHandler(Computer, GameDeck, GameDifficulty);
        }

        /// <summary>Start the game and prepare the initial table card.</summary>
        public void StartGame()
        {
            LogicCard = GameDeck.PreventInitialSpecialCard();

            if (LogicCard is not null)
            {
                GameDeck.AddToDiscardPile(LogicCard);
                VisualCard = LogicCard;

                IsPlayerTurn = true;
            }
            else
                return;
        }

        /// <summary>Advance the game by one turn (player or computer).</summary>
        /// <param name="playerDecision">Player input/command</param>
        /// <returns>UI message produced during the turn</returns>
        public string AdvanceTurn(string playerDecision = "")
        {
            PlayerSkipped = false;
            ComputerSkipped = false;

            string uiMessage = "";

            // --------------- PLAYER'S TURN -------------- // 

            if (IsPlayerTurn && (LogicCard is not null && VisualCard is not null))
            {
                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, playerDecision, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (isSuccessful)
                {
                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        ComputerSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard, ComputerSkipped, GameDeck, Computer, PenaltyCard);
                    }

                    if (!ComputerSkipped)
                        IsPlayerTurn = false;
                }
            }

            // --------------- COMPUTER'S TURN -------------- // 

            else if (!IsPlayerTurn && (LogicCard is not null && VisualCard is not null))
            {
                var (message, cardPlayed) = _computerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, Player.Hand.Count, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;
                    PlayerSkipped = GameMethods.ApplySpecialCardEffect(LastPlayedCard, PlayerSkipped, GameDeck, Player, PenaltyCard);
                }

                if (!PlayerSkipped)
                    IsPlayerTurn = true;
            }
            
            return uiMessage;
        }
    }
}