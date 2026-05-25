using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SolitaireUno
{
    public class GameIntroduction
    {
        public static void ShowGameIntroduction()
        {
            IInputProvider realInput = new ConsoleInput();
            IOutputProvider realOutput = new ConsoleOutput();
            
            string PlayerGameModeChoice;
            string SuitEnforcementChoice;
            string PlayerDifficultyChoice;

            bool EnforceSuits = false;
            
            GameMode chosenGameMode;
            GameDifficulty chosenDifficulty = GameDifficulty.Easy;
            
            realOutput.WriteLine(@"   
                  _____       _ _ _        _           _    _             
                 / ____|     | (_) |      (_)         | |  | |            
                | (___   ___ | |_| |_ __ _ _ _ __ ___ | |  | |_ __   ___  
                 \___ \ / _ \| | | __/ _` | | '__/ _ \| |  | | '_ \ / _ \ 
                 ____) | (_) | | | || (_| | | | |  __/| |__| | | | | (_) |
                |_____/ \___/|_|_|\__\__,_|_|_|  \___| \____/|_| |_|\___/ 
                ");
            
            realOutput.Write("Are You Ready to Play? ");
            string playerChoice = realInput.GetInput().ToLower().Trim();

            switch (playerChoice)
            {
                case "yes" or "y":
                    bool validSuitEnforcementChoice = false;
                    while (!validSuitEnforcementChoice)
                    {
                        realOutput.Write("\nOkay, Would You Like to Enable Suit Enforcement? ");
                        SuitEnforcementChoice = realInput.GetInput().ToLower().Trim();

                        if (SuitEnforcementChoice.Equals("yes") || SuitEnforcementChoice.Equals("y"))
                        {
                            realOutput.WriteLine("Okay! Remember, Reds on Blacks and Blacks on Reds. (Black: Spades/Clubs | Red: Hearts/Diamonds)");

                            EnforceSuits = true;
                            validSuitEnforcementChoice = true;
                        }

                        else if (SuitEnforcementChoice.Equals("no") || SuitEnforcementChoice.Equals("n"))
                        {
                            validSuitEnforcementChoice = true;
                        }

                        else
                        {
                            realOutput.WriteLine("Please answer again, there may have been a mistake in your response");
                        }
                    }

                    bool validModeChoice = false;
                    while (!validModeChoice)
                    {
                        realOutput.Write("\nNow... Would You Like to Play the Cards in Ascending (a) or Descending (d) Order? ");
                        PlayerGameModeChoice = realInput.GetInput().ToLower().Trim();

                        if (PlayerGameModeChoice.Equals("ascending") || PlayerGameModeChoice.Equals("descending") || PlayerGameModeChoice.Equals("a") || PlayerGameModeChoice.Equals("d"))
                        {
                            if (PlayerGameModeChoice.Equals("ascending") || PlayerGameModeChoice.Equals("a"))
                            {
                                validModeChoice = true;
                                chosenGameMode = GameMode.Ascending;
                            }

                            else
                            {
                                validModeChoice = true;
                                chosenGameMode = GameMode.Descending;
                            }

                            bool validChosenDifficulty = false;
                            while (!validChosenDifficulty)
                            {
                                realOutput.Write("\nLastly, What Difficulty Can You Endure... (Easy (m), Medium (m), or Hard (h))? ");

                                PlayerDifficultyChoice = realInput.GetInput().ToLower().Trim();

                                switch (PlayerDifficultyChoice)
                                {
                                    case "easy" or "e":
                                        realOutput.WriteLine("Easy? Lame...");
                                        chosenDifficulty = GameDifficulty.Easy;

                                        validChosenDifficulty = true;
                                        break;

                                    case "medium" or "m":
                                        realOutput.WriteLine("Medium? You like a little spice I see...");
                                        chosenDifficulty = GameDifficulty.Medium;

                                        validChosenDifficulty = true;
                                        break;

                                    case "hard" or "h":
                                        realOutput.WriteLine("Hard?! Mama didn't raise a punk I see!");
                                        chosenDifficulty = GameDifficulty.Hard;

                                        validChosenDifficulty = true;
                                        break;

                                    default:
                                        realOutput.WriteLine("That isn't a valid response.");
                                        break;
                                }
                            }

                            Deck normalDeck = new Deck();
                            MainGame newGame = new MainGame(realInput, realOutput, normalDeck, chosenGameMode, EnforceSuits, chosenDifficulty);
                            newGame.StartGame();
                        }

                        else
                        {
                            realOutput.WriteLine("Please answer again, there may have been a mistake in your response");
                        }
                    }

                    break;


                case "no" or "n":
                    realOutput.WriteLine("I understand, come back when you are ready");
                    break;

                default:
                    realOutput.WriteLine("What?");
                    break;
            }
        }
    }
}
