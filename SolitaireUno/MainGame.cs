using System.Text;

namespace SolitaireUno
{
    public class MainGame
    {
        public Player Player { get; private set; } // property represening the human player
        public Computer Computer { get; private set; } // property representing the computer(s)
        public Deck GameDeck { get; set; } // represents the game deck

        internal PlayerTurnHandler _playerTurnHandler; // represents the turn of the human player
        internal ComputerTurnHandler _computerTurnHandler; // represents the turn of the computer

        public GameMode GameModeChoice { get; set; } // represents the game mode
        internal GameDifficulty GameDifficulty { get; set; } // represents the game difficulty

        public bool IsPlayerTurn { get; set; } // represents whether its the human players turn
        internal bool SuitEnforcement { get; private set; } // represents whether suits are enforced
        public bool ComputerSkipped { get; set; } // represents if the computer has been skipped
        public bool PlayerSkipped { get; set; } // represents if the player has been skipped

        public Card? LastPlayedCard { get; private set; } // represents the last played card
        public Card? LogicCard; // represents the logical card
        public Card? VisualCard; // represents the visual card
        internal RegularCard PenaltyCard { get; private set; } // represents the penalty card

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

        /// <summary>
        /// Starts the game, handles addtional deck setup if needed, and starts the player's turn.
        /// </summary>
        public void StartGame()
        {
            LogicCard = GameDeck.DealCard()!;
            GameDeck.AddToDiscardPile(LogicCard);

            Card? updatedCard = GameMethods.PreventInitalSpecialCard(LogicCard, GameDeck);
            if (updatedCard is not null)
            {
                LogicCard = updatedCard;
                VisualCard = updatedCard;
            }
            else
            {
                VisualCard = LogicCard;
            }

            IsPlayerTurn = true;
        }

        /// <summary>
        /// This method alternates between the player and computer(s) turns
        /// </summary>
        /// <param name="playerDecision"></param>
        /// <returns></returns>
        public string AdvanceTurn(string playerDecision = "")
        {
            // setting both the computer and player skipped to False
            PlayerSkipped = false;
            ComputerSkipped = false;

            // setting the message to empty string
            string uiMessage = "";


            // if its the players turn and both the logic and visal card properly exist, go through the players turn
            if (IsPlayerTurn && (LogicCard is not null && VisualCard is not null))
            {
                var (isSuccessful, message, cardPlayed) = _playerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, playerDecision, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (isSuccessful)
                {
                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        ComputerSkipped = GameMethods.PotentialPlayerAction(LastPlayedCard, ComputerSkipped, GameDeck, Computer, PenaltyCard);
                    }

                    if(!ComputerSkipped)
                        IsPlayerTurn = false;
                }
            }

            else if (!IsPlayerTurn && (LogicCard is not null && VisualCard is not null))
            {
                var (message, cardPlayed) = _computerTurnHandler.HandleTurn(ref LogicCard, ref VisualCard, PenaltyCard, Player.Hand.Count, GameModeChoice, SuitEnforcement);

                uiMessage = message;

                if (cardPlayed is not null)
                {
                    LastPlayedCard = cardPlayed;
                    PlayerSkipped = GameMethods.PotentialComputerAction(LastPlayedCard, PlayerSkipped, GameDeck, Player, PenaltyCard);
                }

                if(!PlayerSkipped)
                    IsPlayerTurn = true;
            }
            
            return uiMessage;
        }
    }
}
