using Microsoft.AspNetCore.Components;

namespace SolitaireUno.Web.Pages
{
    /*
     Home.razor.cs

     Purpose:
     - Blazor component code-behind for the Home page. Hosts the UI-facing game loop
       interactions and provides helper methods for card image selection, hand sorting,
       simulated delays, and checking win conditions.

     Commenting guidelines applied here:
     - Public or important members have XML documentation for IntelliSense.
     - Removed inline end-of-line comments; their intent is described above the relevant
       code blocks as block comments to keep the implementation lines uncluttered.
    */

    public partial class Home : ComponentBase
    {
        // ============= ALL PROPERTIES ============= //

        /// <summary>
        /// The active game engine instance. Null when no game is running.
        /// </summary>
        private MainGame? gameEngine;

        /// <summary>
        /// Whether a game has been started and cards are dealt.
        /// </summary>
        private bool gameStarted = false;

        /// <summary>
        /// Tracks when an AI/computer player is performing its turn to disable human input.
        /// </summary>
        private bool aComputerIsThinking = false;

        /// <summary>
        /// Whether suits are enforced in the current game settings.
        /// </summary>
        private bool suitEnforcement = false;

        private string gameOverMessage = string.Empty;
        private string actionLog = "Welcome to Solitaire Uno!";

        private GameMode selectedMode = GameMode.Ascending;
        private GameDifficulty selectedDifficulty = GameDifficulty.Easy;
        private string selectedCardColor = "yellow";
        private string selectedSortMethod = "None";

        private int selectedPlayerCount = 1;

        /*
         Returns the human player instance from the game engine's AllPlayers list.
         The code looks up a player whose Name equals "Human". If the engine is null,
         this property returns null.
        */
        public Player? HumanPlayer
        {
            get
            {
                if (gameEngine is null)
                    return null;

                return gameEngine?.AllPlayers.First(p => p.Name == "Human");
            }
        }

        /*
         Indicates whether it is currently the human player's turn.
         Compares the player at CurrentTurnIndex to the expected "Human" player name.
        */
        public bool IsHumanTurn
        {
            get
            {
                if (gameEngine is null)
                    return false;

                return gameEngine?.AllPlayers[gameEngine.CurrentTurnIndex].Name == "Human";
            }
        }


        // =================== STARTGAME() ==================== // 

        /// <summary>
        /// Initializes and starts a new game using the selected settings.
        /// Deals a fresh deck into a new MainGame instance and prepares the UI state.
        /// </summary>
        private void StartNewGame()
        {
            gameOverMessage = string.Empty;
            aComputerIsThinking = false;

            GameSettings currentGameSettings = new GameSettings(selectedMode, selectedDifficulty, suitEnforcement, selectedPlayerCount);

            Deck freshDeck = new Deck(currentGameSettings.Mode);

            gameEngine = new MainGame(freshDeck, currentGameSettings);
            gameEngine.StartGame();

            // Apply the currently selected client-side sort method to the human player's hand
            SortHand();

            gameStarted = true;
            actionLog = "Cards dealt. It is your turn.";
        }

        // =================== PLAYTURN() ==================== //

        /// <summary>
        /// Handles a play attempt by the human player and advances the game until
        /// it returns control to the human (after other players/computers have taken
        /// their turns).
        /// </summary>
        /// <param name="decision">A string describing the player's decision (card id or action).</param>
        private async Task PlayTurn(string decision)
        {
            if (gameEngine is null)
                return;

            /*
             If the AI is thinking or the game has ended, ignore input.
             This prevents race conditions and multiple actions from being processed
             while a computer turn is resolving or the game is finished.
            */
            if (aComputerIsThinking || gameOverMessage != string.Empty)
                return;


            // -------------------------- PLAYER'S TURN ----------------------------- //
            /*
             Record that the player's turn is beginning for the UI/log. The game engine
             will return a message and success flag describing the play result.
            */
            if (IsHumanTurn)
                actionLog += "\n\nYour turn.";

            // Holds the amount of cards the human player had before playing a turn
            int? playerHandCountBeforeEveryoneGoes = HumanPlayer?.Hand.Count;

            (string message, bool successfulDecision) humanTurnResult = gameEngine.AdvanceTurn(decision);

            if (!string.IsNullOrEmpty(humanTurnResult.message))
                await UpdateMessageAndUI(humanTurnResult.message);

            if (!humanTurnResult.successfulDecision)
                return;

            // Check if the human player has emptied their hand and thus won.
            Player? winner = ShowWinCondition();
            if (winner is not null)
            {
                gameOverMessage = "You have won! 🎉";
                return;
            }


            // -------------------------- COMPUTER'S TURN ----------------------------- //
            /*
             Continue advancing turns while it is not the human's turn and the game
             has not been won. This loop will let each computer player act in sequence
             by calling AdvanceTurn with an empty decision string.
            */
            while (IsHumanTurn == false && gameOverMessage == string.Empty)
            {
                aComputerIsThinking = true;

                await UpdateMessageAndUI($"{gameEngine?.AllPlayers[gameEngine.CurrentTurnIndex].Name} is thinking...");

                (string message, bool successfulDecision) = gameEngine.AdvanceTurn("");

                if (!string.IsNullOrEmpty(message))
                    await UpdateMessageAndUI(message);

                if (!successfulDecision)
                    return;

                // Check if a computer player has just won
                winner = ShowWinCondition();
                if (winner is not null)
                {
                    gameOverMessage = $"{winner.Name} has won! 🎉";
                    return;
                }

                aComputerIsThinking = false;
            }


            // Holds the amount of cards human player had after everyone went
            int? playerHandCountAfterEveryoneGoes = HumanPlayer?.Hand.Count;

            /*
             If the human player has more cards than before the round and the user has
             chosen a sorting method, re-apply sorting so the UI reflects the chosen order.
            */
            if (playerHandCountAfterEveryoneGoes > playerHandCountBeforeEveryoneGoes && selectedSortMethod is not null)
                SortHand();

            await UpdateMessageAndUI("Your turn");
        }




        // ========================== BLAZOR METHODS ========================= //


        /// <summary>
        /// Sets the proper card image path for a given card based on its concrete type
        /// (RegularCard or SpecialCard) and the currently selected color when applicable.
        /// </summary>
        /// <param name="card">The card to set the image of</param>
        /// <returns>Relative path to the card image or an empty string if the card is null</returns>
        private string GetCardImagePath(Card? card)
        {
            if (card is null)
                return string.Empty;

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
        /// Uses the player's SortHand methods to sort hand cards based on the user's selection.
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
        }

        /// <summary>
        /// Creates a fake pause to simulate real-time thinking for AI players.
        /// </summary>
        /// <returns>A task that completes after a randomized delay.</returns>
        private static async Task SimulatedDelay()
        {
            Random random = new Random();
            int randomWaitingPeriod = (random.Next(7) + 4) * 400;

            await Task.Delay(randomWaitingPeriod);
        }

        /// <summary>
        /// Updates the action log message, triggers a re-render and simulates a short delay.
        /// </summary>
        /// <param name="message">Message to display to the user</param>
        private async Task UpdateMessageAndUI(string message)
        {
            actionLog = message;
            StateHasChanged();
            await SimulatedDelay();
        }

        /// <summary>
        /// Scans all players to see if any player has zero cards left, indicating a win.
        /// </summary>
        /// <returns>The winning player or null if nobody has won yet.</returns>
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
