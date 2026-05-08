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


        public MainGame(Deck deck, GameMode gameModeChoice, bool suitEnforcement, GameDifficulty gameDifficulty)
        {
            Player = new Player();
            Computer = new Computer();
            PenaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            GameDeck = deck;
            GameModeChoice = gameModeChoice;
            GameDifficulty = gameDifficulty;
            SuitEnforcement = suitEnforcement;

            _playerTurnHandler = new PlayerTurnHandler(Player, GameDeck);
            _computerTurnHandler = new ComputerTurnHandler(Computer, GameDeck, GameDifficulty);

            GameSetup.SetupGame(Player, Computer, GameDeck);
        }

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
        public string AdvanceTurn(string playerDecision = "")
        {
            PlayerSkipped = false;
            ComputerSkipped = false;

            string uiMessage = "";

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
