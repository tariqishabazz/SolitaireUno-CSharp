using System.Collections;
using System.Linq;

namespace SolitaireUno
{
    public class Computer : Player
    {
        public Card? MakeMove(Card currentCard, int opponentHandSize, int currentDeckSize, GameDifficulty gameDifficulty)
        {
            Random random = new Random();
            List<Card> validMoves = new List<Card>();
            
            validMoves.AddRange(from Card card in Hand
                                where GameMethods.ValidCard(card, currentCard, MainGame.GameModeChoice)
                                select card);
            
            if (validMoves.Count == 0)
                return null;
            
            List<Card> regularMoves = validMoves.Where(card => card is RegularCard).ToList();
            List<Card> specialMoves = validMoves.Where(card => card is SpecialCard).ToList();

            switch (gameDifficulty)
            {
                case GameDifficulty.Easy:
                    
                    Card randomEasyMove = validMoves[random.Next(validMoves.Count)];
                    return randomEasyMove;

                case GameDifficulty.Medium:
                    
                    if(currentDeckSize <= 7 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];
                        return randomSpecialMove;
                    }

                    switch (opponentHandSize)
                    {
                        case <= 8:
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

                        default:
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
                    }
                
                case GameDifficulty.Hard:

                    if (currentDeckSize <= 7 && specialMoves.Count > 0)
                    {
                        Card randomSpecialMove = specialMoves[random.Next(specialMoves.Count)];
                        return randomSpecialMove;
                    }

                    switch (opponentHandSize)
                    {
                        case <= 5:
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
                        default:
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
                    }

                default:
                    return null;
            }
        }
    }
}