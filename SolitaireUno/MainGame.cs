namespace SolitaireUno
{
    /// <summary>
    /// Represents the main game logic and flow for a card game between a human player and the computer.
    /// </summary>
    /// <remarks>
    /// The MainGame class manages the initialization of players and the deck, handles turn-based gameplay, and determines the win condition.
    /// It provides the entry point for starting and running a complete game session via the StartGame method. User interaction is performed through the provided input/output providers.
    /// </remarks>
    public class MainGame
    {
        Player player = new(); // The human player object, manages the player's hand and actions
        Computer computer = new(); // The computer-controlled player, inherits from Player and adds AI logic
        Deck gameDeck = new(); // The deck of cards used for dealing and drawing during the game
        Card currentCard; // The card currently in play, updated after each valid move

        private readonly IInputProvider _input; // Interface for handling user input (e.g., from console or other sources)
        private readonly IOutputProvider _output; // Interface for handling output to the user (e.g., console or GUI)

        private readonly PlayerTurnHandler _playerTurnHandler; // Encapsulates all logic for handling a human player's turn
        private readonly ComputerTurnHandler _computerTurnHandler; // Encapsulates all logic for handling the computer's turn

        private string _GameModeChoice; // Stores the user's selected game mode (e.g., ascending/descending), used for move validation

        public string GameModeChoice 
        {
            get { return _GameModeChoice; }
            set { _GameModeChoice = value; }
        }

        /// <summary>
        /// Initializes a new instance of the MainGame class with input/output providers, deck, and game mode.
        /// Sets up the player, computer, deck, and initializes turn handlers.
        /// </summary>
        /// <param name="input">The input provider for user actions (e.g., ConsoleInput).</param>
        /// <param name="output">The output provider for displaying messages (e.g., ConsoleOutput).</param>
        /// <param name="deck">The deck to use for the game session, shuffled and ready.</param>
        /// <param name="gameModeChoice">The user's selected game mode (e.g., ascending/descending), used for move validation.</param>
        public MainGame(IInputProvider input, IOutputProvider output, Deck deck, string gameModeChoice)
        {
            _input = input; // Assign the input provider for user actions
            _output = output; // Assign the output provider for displaying messages
            gameDeck = deck; // Assign the provided deck to the game instance
            _GameModeChoice = gameModeChoice; // Store the user's game mode choice for later use

            _playerTurnHandler = new(player, gameDeck, _input, _output); // Create the handler for the player's turn logic
            _computerTurnHandler = new(computer, gameDeck, _output); // Create the handler for the computer's turn logic

            InitialGameSetup.SetupGame(player, computer, gameDeck); // Deal initial hands and prepare the deck for play
        }
        
        /// <summary>
        /// Starts the main game loop, alternating turns between the player and computer until a win condition is met.
        /// Handles the penalty card logic and updates the current card after each move.
        /// </summary>
        /// <returns>The winning player (human or computer), or null if no winner (should not occur).</returns>
        public Player? StartGame()
        {
            Card penaltyCard = new(Suits.Spades, Values.Queen); // Define the penalty card (Queen of Spades) for special penalty logic
            
            currentCard = gameDeck.DealCard()!; // Deal the first card from the deck to start the game

            // Main game loop: continue until either the player or computer runs out of cards
            while (player.Hand.Count > 0 && computer.Hand.Count > 0)
            {
                ShowHand(); // Display the player's current hand and card positions
                _playerTurnHandler.HandleTurn(ref currentCard, penaltyCard); // Execute the player's turn, updating the current card
                _computerTurnHandler.HandleTurn(ref currentCard, penaltyCard); // Execute the computer's turn, updating the current card
            }

            /* --------------------- WIN CONDITION ---------------------- */

            _output.WriteLine("\n\n\n-------------------------------------------------------------"); // Print a separator for clarity
            _output.WriteLine("Game Over!"); // Announce the end of the game

            // Determine and announce the winner based on who ran out of cards first
            if(computer.Hand.Count == 0) // If computer has no cards left, player loses
            {
                _output.WriteLine("\nYou Lose! You've been bested by the machine :("); // Output loss message
                return computer; // Return computer as the winner
            }
            else if(player.Hand.Count == 0) // If player has no cards left, player wins
            {
                _output.WriteLine("\nYou Win! You beat the computer! Congrats! :)"); // Output win message
                return player; // Return player as the winner
            }

            return null; // No winner (should not occur in normal play)
        }

        /* --------------------- INTRO CONVERSATION ---------------------- */
        /// <summary>
        /// Entry point for the application. Starts the game introduction and setup sequence.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        static void Main(string[] args)
        {
            GameIntroduction.ShowGameIntroduction(); // Start the game introduction and setup
        }

        // ------------------------------------------- Game Methods ----------------------------------------------

        /// <summary>
        /// Outputs the player's current hand to the output provider, showing each card with its index for user reference.
        /// </summary>
        public void ShowHand()
        {
            _output.WriteLine("\nYour Hand: "); // Print a header for the hand display

            int index = 0; // Initialize card index for display
            foreach (Card card in player.Hand) // Loop through each card in the player's hand
            {
                _output.WriteLine($"   {index + 1}) {card}"); // Print the card with its position (1-based index)
                index++; // Move to the next card index
            }
        }
    }
}

// Add new game modes/card types?
// Add new GameMethods test for new logic?