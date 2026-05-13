using System.Collections;
using System.Linq;

namespace SolitaireUno
{
    public class Computer(Deck gameDeck) : Player(gameDeck)
    {
        public Card? MakeMove(Card logicCard, int opponentHandSize, int currentDeckSize, GameDifficulty gameDifficulty, GameMode gameMode, bool suitEnforcement)
        {
            Random random = new Random();

            //The two dots are called the spread operator. In this case,
            //it takes all the individual cards found by your query and "spreads"
            //them into the new validMoves list.
            List<Card> validMoves =
            [
                .. from Card potentialCard in Hand
                                    where CardValidation.ValidCard(potentialCard, logicCard, gameMode, suitEnforcement)
                                    select potentialCard,
            ];

            if (validMoves.Count == 0)
                return null;

            List<Card> regularMoves = [.. validMoves.Where(card => card is RegularCard)];
            List<Card> specialMoves = [.. validMoves.Where(card => card is SpecialCard)];

            switch (gameDifficulty)
            {
                case GameDifficulty.Easy:

                    Card randomEasyMove = validMoves[random.Next(validMoves.Count)];
                    return randomEasyMove;

                case GameDifficulty.Medium:

                    if (currentDeckSize <= 7 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];
                        return randomSpecialMove;
                    }

                    if (opponentHandSize <= 7)
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

                    if (currentDeckSize <= 7 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];
                        return randomSpecialMove;
                    }

                    if (opponentHandSize <= 5)
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

                default:
                    return null;
            }
        }
    }
}