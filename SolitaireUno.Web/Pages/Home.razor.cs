using Microsoft.AspNetCore.Components;
using System.Globalization;
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
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private TimeSpan _gameUptime = TimeSpan.Zero;

        // ============= ALL GAME PROPERTIES ============= //

        private MainGame? gameEngine;
        private GameMode selectedMode = GameMode.Ascending;
        private GameDifficulty selectedDifficulty = GameDifficulty.Easy;

        private bool gameStarted = false;
        private bool suitEnforcement = false;

        private string gameOverMessage = string.Empty;
        private string actionLog = "Welcome to Solitaire Uno!";
        private string selectedCardColor = "yellow";
        private string selectedSortMethod = "None";
        private string playerName = string.Empty;

        private int selectedPlayerCount = 1;

        public Player? HumanPlayer
        {
            get
            {
                if (gameEngine is null)
                    return null;

                return gameEngine.AllPlayers.First(player => player.Name == playerName);
            }
        }

        public Player? ComputerPlayer
        {
            get
            {
                if (gameEngine is null || gameEngine.CurrentState is null)
                    return null;

                return gameEngine.AllPlayers[ gameEngine.CurrentState.CurrentTurnIndex ];
            }
        }

        public bool IsHumanTurn
        {
            get
            {
                if (gameEngine is null || gameEngine?.CurrentState is null)
                    return false;

                return gameEngine?.AllPlayers[ gameEngine.CurrentState.CurrentTurnIndex ].Name == playerName;
            }
        }

        public bool IsComputerTurn
        {
            get
            {
                if (gameEngine is null || gameEngine?.CurrentState is null)
                    return false;

                return gameEngine.AllPlayers[ gameEngine.CurrentState.CurrentTurnIndex ] is Computer;
            }
        }


        // =================== STARTGAME() ==================== //

        private async void StartNewGame()
        {
            gameOverMessage = string.Empty;
            _gameUptime = TimeSpan.Zero;

            playerName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerName);

            GameSettings currentGameSettings = new(selectedMode, selectedDifficulty, suitEnforcement, selectedPlayerCount, playerName);
            Deck freshDeck = new(currentGameSettings.Mode);

            gameEngine = new MainGame(freshDeck, currentGameSettings);
            gameEngine.StartGame();

            // Apply the currently selected client-side sort method to the human player's hand
            SortHand();

            gameStarted = true;

            if (gameEngine is not null && gameEngine.CurrentState is not null)
                actionLog = $"Cards Dealt. It is {gameEngine.AllPlayers[ gameEngine.CurrentState.CurrentTurnIndex ].Name}'s turn.";

            if (IsComputerTurn)
                await PlayComputerTurn();
        }

        private async Task PlayComputerTurn()
        {
            if (!IsComputerTurn)
                return;

            if (gameEngine is null || gameEngine.CurrentState is null || ComputerPlayer is null)
                return;

            while (!IsHumanTurn && string.IsNullOrEmpty(gameOverMessage))
            {
                await UpdateMessageAndUI($"{gameEngine?.AllPlayers[ gameEngine.CurrentState.CurrentTurnIndex ].Name} is thinking...");

                if (gameEngine is null)
                    return;

                (string message, bool successfulDecision) = gameEngine.AdvanceTurn(string.Empty);

                if (!string.IsNullOrEmpty(message))
                    await UpdateMessageAndUI(message);

                if (LongUIMessage(message) && gameOverMessage != "")
                    await Task.Delay(5000);

                if (!successfulDecision)
                    return;
            }

            Player? potentialWinner = ShowWinCondition();
            if (IsHumanTurn && potentialWinner is null)
                await UpdateMessageAndUI("Your Turn");
        }

        private async Task PlayHumanTurn(string decision)
        {
            if (gameEngine is null || gameEngine.CurrentState is null || HumanPlayer is null)
                return;

            if (!IsHumanTurn || !string.IsNullOrEmpty(gameOverMessage))
                return;

            int playerHandCountBeforeEveryoneGoes = HumanPlayer.Hand.Count;

            (string message, bool successfulDecision) = gameEngine.AdvanceTurn(decision);

            if (!string.IsNullOrEmpty(message))
                await UpdateMessageAndUI(message);

            if (LongUIMessage(message) && gameOverMessage != "")
                await Task.Delay(5000);

            if (!successfulDecision)
                return;

            int playerHandCountAfterEveryoneGoes = HumanPlayer.Hand.Count;

            if (playerHandCountAfterEveryoneGoes > playerHandCountBeforeEveryoneGoes && selectedSortMethod is not null)
                SortHand();

            Player? potentialWinner = ShowWinCondition();
            if (IsComputerTurn && potentialWinner is null)
                await PlayComputerTurn();
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
            if (HumanPlayer is not null)
            {
                PlayerHandSorter handSorter = new(HumanPlayer);

                switch (selectedSortMethod)
                {
                    case "Values":
                        handSorter.SortHandByValue();
                        break;

                    case "Suits":
                        handSorter.SortHandBySuit();
                        break;

                    case "By Suits, then Values":
                        handSorter.SortHandBySuitAndValue();
                        break;

                    default:
                        break;
                }
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
            {
                if (player.Hand.Count > 0)
                    continue;

                if (player.Name == playerName)
                {
                    gameOverMessage = "You have Won! 🎉";
                    return player;
                }

                gameOverMessage = $"{player.Name} has Won! 🎉";
                return player;
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the returned UI message is lengthy, 
        /// and leaves on screen for longer to ensure proper reading
        /// </summary>
        /// <param name="message"></param>
        /// <returns>A bool on whether the message is long</returns>
        private static bool LongUIMessage(string message)
        {
            StringBuilder currentSentence = new StringBuilder();
            int numberOfWords = 0;

            for (int character = 0; character < message.Length; character++)
            {
                if (char.IsLetterOrDigit(message[ character ]) || char.IsPunctuation(message[ character ]))
                {
                    currentSentence.Append(message[ character ]);
                }

                if (char.IsWhiteSpace(message[ character ]))
                {
                    currentSentence.Append(message[ character ]);
                    numberOfWords++;
                }
            }

            return numberOfWords >= 7;
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
    }
}