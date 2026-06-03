using Microsoft.AspNetCore.Components;
using System.Security.Cryptography.X509Certificates;

namespace SolitaireUno.Web.Pages
{
    public partial class Home : ComponentBase
    {

        // ============= ALL PROPERTIES ============= //

        private MainGame? gameEngine;

        private bool gameStarted = false;
        private bool aComputerIsThinking = false;
        private bool suitEnforcement = false;

        private string gameOverMessage = "";
        private string actionLog = "Welcome to Solitaire Uno!";

        private GameMode selectedMode = GameMode.Ascending;
        private GameDifficulty selectedDifficulty = GameDifficulty.Easy;
        private string selectedCardColor = "yellow";
        private string selectedSortMethod = "";

        private int selectedPlayerCount = 1;

        public Player? HumanPlayer
        {
            get
            {
                if (gameEngine is null)
                    return null;

                return gameEngine?.AllPlayers.First(p => p.Name == "Human"); // grabbing the human player
            }
        }

        public bool IsHumanTurn
        {
            get
            {
                if (gameEngine is null)
                    return false;

                return gameEngine?.AllPlayers[gameEngine.CurrentTurnIndex].Name == "Human"; // seeing if its the humans turn based on the turn index
            }
        }


        // =================== STARTGAME() ==================== // 

        private void StartNewGame()
        {
            gameOverMessage = "";
            aComputerIsThinking = false;

            Deck freshDeck = new Deck();

            GameSettings currentGameSettings = new GameSettings(selectedMode, selectedDifficulty, suitEnforcement, selectedPlayerCount);

            gameEngine = new MainGame(freshDeck, currentGameSettings);
            gameEngine.StartGame();

            SortHand();

            gameStarted = true;
            actionLog = "Cards dealt. It is your turn.";
        }

        // =================== PLAYTURN() ==================== //

        private async Task PlayTurn(string decision)
        {
            if (gameEngine is null)
                return;

            // disables human input/actions if its a computers turn, or the game is over
            if (aComputerIsThinking || gameOverMessage != "")
                return;


            // -------------------------- PLAYER'S TURN ----------------------------- //            
            if (IsHumanTurn)
                actionLog += "\n\nYour turn.";

            // holds amount of cards the human player had before playing a turn
            int? playerHandCountBeforeEveryoneGoes = HumanPlayer?.Hand.Count;

            (string message, bool successfulDecision) humanTurnResult = gameEngine.AdvanceTurn(decision); 

            if (!string.IsNullOrEmpty(humanTurnResult.message))
                await UpdateMessageAndUI(humanTurnResult.message);

            if (!humanTurnResult.successfulDecision)
                return;

            // check if human won
            Player? winner = ShowWinCondition();
            if (winner is not null)
            {
                gameOverMessage = "You have won! 🎉";
                return;
            }


            // -------------------------- COMPUTER'S TURN ----------------------------- //

            while (IsHumanTurn == false && gameOverMessage == string.Empty)
            {
                aComputerIsThinking = true;

                await UpdateMessageAndUI($"{gameEngine?.AllPlayers[gameEngine.CurrentTurnIndex].Name} is thinking...");

                (string message, bool successfulDecision) = gameEngine.AdvanceTurn("");

                if (!string.IsNullOrEmpty(message))
                    await UpdateMessageAndUI(message);

                if (!successfulDecision)
                    return;

                // check if a computer won
                winner = ShowWinCondition();
                if (winner is not null)
                {
                    gameOverMessage = $"{winner.Name} has won! 🎉";
                    return;
                }

                aComputerIsThinking = false;
            }



            // holds the amount of cards human player had after everyone went
            int? playerHandCountAfterEveryoneGoes = HumanPlayer?.Hand.Count;

            // re-sorts the players hand if they have more cards this round than last round
            if (playerHandCountAfterEveryoneGoes > playerHandCountBeforeEveryoneGoes && selectedSortMethod is not null)
                SortHand();

            await UpdateMessageAndUI("Your turn");
        }




        // ========================== BLAZOR METHODS ========================= //


        /// <summary>
        /// Sets the proper card images to each card based on its Suit or Value, or special card context
        /// </summary>
        /// <param name="card">The card to set the image of</param>
        /// <returns></returns>
        private string GetCardImagePath(Card? card)
        {
            if (card is null)
                return "";

            if (card is RegularCard regularCard)
                return $"images/cards/{regularCard.Value.ToString().ToLower()}_of_{regularCard.Suit.ToString().ToLower()}.png";

            else if (card is SpecialCard specialCard)
            {
                switch (specialCard.CardType)
                {
                    case SpecialCardType.DrawFour:
                        return $"images/cards/drawfour.png";

                    case SpecialCardType.DrawTwo:
                        return $"images/cards/{selectedCardColor}_draw_two.png";

                    case SpecialCardType.Skip:
                        return $"images/cards/{selectedCardColor}_skip.png";
                }
            }
            return $"images/cards/{selectedCardColor}_backing.png";
        }

        /// <summary>
        /// Uses the players SortHand methods to properly sort hand cards based on user's desire
        /// </summary>
        private void SortHand()
        {
            switch (selectedSortMethod)
            {
                case "Values":
                    gameEngine?.AllPlayers[0].SortHandByValue();
                    break;

                case "Suits":
                    gameEngine?.AllPlayers[0].SortHandBySuit();
                    break;

                case "Both Suits and Values":
                    gameEngine?.AllPlayers[0].SortHandBySuitAndValue();
                    break;

                default:
                    break;
            }
            ;
        }

        /// <summary>
        /// Creates a fake pause to simulate real-time thinking
        /// </summary>
        /// <returns>A Task Delay</returns>
        private static async Task SimulatedDelay()
        {
            Random random = new Random();
            int randomWaitingPeriod = (random.Next(7) + 4) * 400;

            await Task.Delay(randomWaitingPeriod);
        }

        private async Task UpdateMessageAndUI(string message)
        {
            actionLog = message;
            StateHasChanged();
            await SimulatedDelay();
        }

        private Player? ShowWinCondition()
        {
            if (gameEngine is not null)
            {
                foreach (Player player in gameEngine.AllPlayers)
                {
                    if (player.Hand.Count == 0)
                        return player;
                }
            }
            return null;
        }
    }
}
