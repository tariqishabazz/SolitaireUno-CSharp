using Microsoft.AspNetCore.Components;
using System.Text;

namespace SolitaireUno.Web.Pages
{
    /*
     Home.razor.cs

     Purpose:
     - Blazor component code-behind for the Home page. Hosts the UI-facing game loop
       interactions and provides helper methods for card image selection, hand sorting,
       simulated delays, and checking win conditions.
    */

    public partial class Home : ComponentBase, IDisposable
    {
        // ============= ALL TIMER PROPERTIES ============= //

        private PeriodicTimer? _timer;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private TimeSpan _gameUptime = TimeSpan.Zero;

        // ============= ALL GAME PROPERTIES ============= //

        private MainGame? gameEngine;

        private bool gameStarted = false;
        private bool aComputerIsThinking = false;
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

                return gameEngine?.AllPlayers[gameEngine.CurrentState.CurrentTurnIndex].Name == "Human";
            }
        }

        // =================== STARTGAME() ==================== //

        /// <summary>
        /// Initializes and starts a new game using the selected settings.
        /// Deals a fresh deck into a new MainGame instance and prepares the UI state.
        /// </summary>
        private void StartNewGame()
        {
            /* Reset game states and properties to their initial values.
             *      This ensures that starting a new game
             *      will clear any previous game data and UI messages. */

            gameOverMessage = string.Empty;
            aComputerIsThinking = false;
            _gameUptime = TimeSpan.Zero;

            // Create and start game with desired configurations //

            GameSettings currentGameSettings = new GameSettings(selectedMode, selectedDifficulty, suitEnforcement, selectedPlayerCount);
            Deck freshDeck = new Deck(currentGameSettings.Mode);
            gameEngine = new MainGame(freshDeck, currentGameSettings);
            gameEngine.StartGame();

            // Apply the currently selected client-side sort method to the human player's hand
            SortHand();

            gameStarted = true;
            actionLog = "Cards Dealt. It is your turn.";
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

            // ===================== PLAYER'S TURN ===================== //
            /*
                 Record that the player's turn is beginning for the UI/log. The game engine
                 will return a message and success flag describing the play result.
            */
            if (IsHumanTurn)
                actionLog += "\n\nYour Turn.";

            // Holds the amount of cards the human player had before playing a turn
            int? playerHandCountBeforeEveryoneGoes = HumanPlayer?.Hand.Count;

            (string message, bool successfulDecision) humanTurnResult = gameEngine.AdvanceTurn(decision);

            if (!string.IsNullOrEmpty(humanTurnResult.message))
                await UpdateMessageAndUI(humanTurnResult.message);

            if (LongUIMessage(humanTurnResult.message) && gameOverMessage != string.Empty)
                await Task.Delay(5000);

            if (!humanTurnResult.successfulDecision)
                return;

            // Check if the human player has emptied their hand and thus won.
            Player? winner = ShowWinCondition();
            if (winner is not null)
            {
                gameOverMessage = "You have Won! 🎉";
                return;
            }

            // ===================== COMPUTER'S TURN ===================== //

            /*
             Continue advancing turns while it is not the human's turn and the game
             has not been won. This loop will let each computer player act in sequence
             by calling AdvanceTurn with an empty decision string.
            */
            while (IsHumanTurn == false && gameOverMessage == string.Empty)
            {
                aComputerIsThinking = true;

                await UpdateMessageAndUI($"{gameEngine?.AllPlayers[gameEngine.CurrentState.CurrentTurnIndex].Name} is thinking...");

                if (gameEngine is not null)
                {
                    (string message, bool successfulDecision) = gameEngine.AdvanceTurn(string.Empty);

                    if (!string.IsNullOrEmpty(message))
                        await UpdateMessageAndUI(message);

                    if (LongUIMessage(message) && gameOverMessage != string.Empty)
                        await Task.Delay(5000);

                    if (!successfulDecision)
                        return;
                }

                // Check if a computer player has just won
                winner = ShowWinCondition();
                if (winner is not null)
                {
                    gameOverMessage = $"{winner.Name} has Won! 🎉";
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

            await UpdateMessageAndUI("Your Turn");
        }

        // ========================== HELPER METHODS ========================= //

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
                return specialCard.CardType switch
                {
                    SpecialCardType.DrawFour => $"images/cards/draw_four.png",
                    SpecialCardType.DrawTwo => $"images/cards/{selectedCardColor}_draw_two.png",
                    SpecialCardType.Skip => $"images/cards/{selectedCardColor}_skip.png",
                    SpecialCardType.Reverse => $"images/cards/{selectedCardColor}_reverse.png",
                    _ => $"Card image not found"
                };
            }

            return $"Card image not found";
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

                case "By Suits, then Values":
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
            Random random = Random.Shared;
            int waitTime = 1700 + (random.Next() % 650);

            await Task.Delay(waitTime);
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
            if (gameEngine is null)
                return null;

            foreach (Player player in gameEngine.AllPlayers)
                if (player.Hand.Count == 0)
                    return player;

            return null;
        }

        /* ================== TIMER METHODS ================= */

        protected override void OnInitialized()
        {
            Task.Run(BackgroundTimerLoop);
        }

        private async Task BackgroundTimerLoop()
        {
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (gameStarted && string.IsNullOrEmpty(gameOverMessage))
                    {
                        _gameUptime = _gameUptime.Add(TimeSpan.FromSeconds(1));

                        _ = InvokeAsync(StateHasChanged);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _timer?.Dispose();
        }

        private static bool LongUIMessage(string message)
        {
            // create new string to hold current word being built from characters in message
            StringBuilder currentSentence = new StringBuilder();

            int numberOfWords = 0;

            // loop over each character in the message to find individual words
            for (int character = 0; character < message.Length; character++)
            {
                // if character is not a space, keep adding to current word string until space is found
                if (char.IsLetterOrDigit(message[character]) || char.IsPunctuation(message[character]))
                {
                    currentSentence.Append(message[character]);
                }

                // if character is a space, add previous index elements to form word in new string
                if (char.IsWhiteSpace(message[character]))
                {
                    currentSentence.Append(message[character]);
                    numberOfWords++;
                }
            }

            return numberOfWords >= 7;
        }
    }
}