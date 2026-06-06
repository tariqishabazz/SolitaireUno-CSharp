/*
 Computer.cs

 Purpose:
 - Computer AI player implementation that selects and returns a valid move
   given the current game state and difficulty.

 Commenting guideline applied:
 - File-level purpose header added to match Home.razor.cs style. Method summaries preserved.
*/

namespace SolitaireUno
{
    /// <summary>
    /// Computer player AI responsible for choosing and playing cards.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Computer"/> class.
    /// </remarks>
    /// <param name="gameDeck">The deck the computer can draw from during setup.</param>
    public class Computer : Player
    {
        /// <summary>
        /// Determines and returns a card the computer will play based on the current game state and AI difficulty.
        /// </summary>
        /// <param name="logicCard">The current logic card on the table used for validation.</param>
        /// <param name="opponentHandSize">The number of cards in the opponent's hand.</param>
        /// <param name="currentDeckSize">The current number of cards remaining in the deck.</param>
        /// <param name="gameDifficulty">The difficulty level that influences move selection.</param>
        /// <param name="gameMode">The current game mode (ascending/descending).</param>
        /// <param name="suitEnforcement">Whether suit enforcement is active for validation.</param>
        /// <returns>The chosen card to play, or null if no valid move exists.</returns>
        public Card? MakeMove(Card logicCard, int opponentHandSize, int currentDeckSize, GameSettings currentGameSettings)
        {
            Random random = new();

            //The two dots are called the spread operator. In this case,
            //it takes all the individual cards found by the query and "spreads"
            //them into the new validMoves list.
            List<Card> validMoves =
            [
                .. from Card potentialCard in Hand
                                    where CardValidation.ValidCard(potentialCard, logicCard, currentGameSettings)
                                    select potentialCard,
            ];

            if (validMoves.Count == 0)
                return null;

            List<Card> regularMoves = [.. validMoves.Where(card => card is RegularCard)];
            List<Card> specialMoves = [.. validMoves.Where(card => card is SpecialCard)];

            // Computer AI switches based on chosen difficulty
            switch (currentGameSettings.Difficulty)
            {
                case GameDifficulty.Easy:

                    Card randomEasyMove = validMoves[random.Next(validMoves.Count)];
                    return randomEasyMove;

                case GameDifficulty.Medium:

                    if (currentDeckSize <= 12 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];

                        if (randomSpecialMove.Equals(SpecialCardType.DrawFour) || randomSpecialMove.Equals(SpecialCardType.DrawTwo))
                            return randomSpecialMove;

                        return randomSpecialMove;
                    }

                    if (opponentHandSize < 7)
                    {
                        if (specialMoves.Count > 0)
                        {
                            Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];

                            if (validMoves.Count == 1 && validMoves[0] is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                                return null;

                            return randomSpecialMove;
                        }

                        else
                        {
                            Card randomRegularMove = regularMoves[random.Next(regularMoves.Count)];
                            return randomRegularMove;
                        }
                    }

                    else
                    {
                        if (regularMoves.Count > 0)
                        {
                            Card randomRegularMove = regularMoves[random.Next(regularMoves.Count)];
                            return randomRegularMove;
                        }

                        else
                        {
                            return null;
                        }
                    }

                case GameDifficulty.Hard:

                    if (currentDeckSize <= 12 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];

                        if (randomSpecialMove.Equals(SpecialCardType.DrawFour) || randomSpecialMove.Equals(SpecialCardType.DrawTwo))
                            return randomSpecialMove;

                        return randomSpecialMove;
                    }

                    if (opponentHandSize < 4)
                    {
                        if (specialMoves.Count > 0)
                        {
                            Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];

                            // makes sure that the computer doesn't play their only good Skip
                            if (validMoves.Count == 1 && validMoves[0] is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                                return null;

                            return randomSpecialMove;
                        }

                        else
                        {
                            Card randomRegularMove = regularMoves[random.Next(regularMoves.Count)];
                            return randomRegularMove;
                        }
                    }

                    else
                    {
                        if (regularMoves.Count > 0)
                        {
                            Card randomRegularMove = regularMoves[random.Next(regularMoves.Count)];
                            return randomRegularMove;
                        }

                        else
                        {
                            return null;
                        }
                    }

                default:
                    return null;
            }
        }
    }
}