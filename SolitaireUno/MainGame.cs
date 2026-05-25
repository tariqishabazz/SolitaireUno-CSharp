using System.Text;

namespace SolitaireUno
{
    public class MainGame
    {
        public static readonly Player player = new();
        public static readonly Computer computer = new();
        internal static Deck GameDeck { get; set; }

        internal static IInputProvider Input { get; set; }
        internal static IOutputProvider Output { get; set; }

        internal PlayerTurnHandler _playerTurnHandler;
        internal ComputerTurnHandler _computerTurnHandler;

        public static GameMode GameModeChoice { get; set; }
        internal static GameDifficulty GameDifficulty { get; set; }

        internal static bool IsPlayerTurn { get; set; }
        internal static bool SuitEnforcement { get; private set; }
        
        internal static Card? LastPlayedCard { get; private set; }


        public MainGame(IInputProvider input, IOutputProvider output, Deck deck, GameMode gameModeChoice, bool suitEnforcement, GameDifficulty gameDifficulty)
        {
            Input = input;
            Output = output;
            GameDeck = deck;
            GameModeChoice = gameModeChoice;
            GameDifficulty = gameDifficulty;
            SuitEnforcement = suitEnforcement;

            _playerTurnHandler = new PlayerTurnHandler(player, GameDeck, Input, Output);
            _computerTurnHandler = new ComputerTurnHandler(computer, GameDeck, Output);

            GameSetup.SetupGame(player, computer, GameDeck);
        }

        public void StartGame()
        {
            RegularCard penaltyCard = new RegularCard(Suits.Spades, Values.Queen);
            Card logicCard = GameDeck.DealCard()!;
            GameDeck.AddToDiscardPile(logicCard);

            Card? updatedCard = GameMethods.PreventInitialSpecialCard(logicCard);

            Card visualCard;
            if (updatedCard is not null)
            {
                logicCard = updatedCard;
                visualCard = updatedCard;
            }
            else
            {
                return;
            }

            IsPlayerTurn = true;

            do
            {
                if (IsPlayerTurn && logicCard is not null)
                {
                    Card? cardPlayed = _playerTurnHandler.HandleTurn(ref logicCard, ref visualCard, penaltyCard);

                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        bool computerSkipped = GameMethods.PotentialPlayerAction();

                        if (computerSkipped)
                            continue;
                    }
                    else
                    {
                        IsPlayerTurn = false;
                    }
                }
                else if (!IsPlayerTurn && logicCard is not null)
                {
                    Card? cardPlayed = _computerTurnHandler.HandleTurn(ref logicCard, ref visualCard, penaltyCard, player.Hand.Count);

                    if (cardPlayed is not null)
                    {
                        LastPlayedCard = cardPlayed;
                        bool playerSkipped = GameMethods.PotentialComputerAction();

                        if (playerSkipped)
                            continue;
                    }
                    else
                    {
                        IsPlayerTurn = true;
                    }
                }
            }
            while (player.Hand.Count > 0 && computer.Hand.Count > 0);

            GameMethods.GameOverStats();
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            GameIntroduction.ShowGameIntroduction();
        }
    }
}
