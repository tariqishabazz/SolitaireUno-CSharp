using SolitaireUno;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

//Console.OutputEncoding = System.Text.Encoding.UTF8;

namespace SolitaireUno
{
    public class GameMethods
    {
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameMode gameMode, bool suitEnforcement)
        {
            if (potentialPlay is RegularCard firstRegularCard && logicCardShown is RegularCard secondRegularCard)
            {
                bool isValidSequence = gameMode == GameMode.Descending
                    ? IsValidDescending(potentialPlay, logicCardShown)
                    : IsValidAscending(potentialPlay, logicCardShown);

                if (!isValidSequence)
                {
                    return false;
                }

                return suitEnforcement ? SameColor(firstRegularCard, secondRegularCard) : true;
            }

            else
            {
                return IsSpecialCard(potentialPlay);
            }
        }

        private static bool IsValidDescending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value - 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Descending);
        }

        private static bool IsValidAscending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value + 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Ascending);
        }

        private static bool IsWrapAround(Card potentalPlay, Card currentlyShown, GameMode gameMode)
        {
            if (potentalPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if (gameMode == GameMode.Descending)
                    return potentialCard.Value == Values.King && currentCard.Value == Values.Ace;
                else
                    return potentialCard.Value == Values.Ace && currentCard.Value == Values.King;
            else
                return false;
        }

        public static bool IsSpecialCard(Card potentialPlay) => potentialPlay is SpecialCard;

        public static int GetPenaltyCount(Card dealtCard, Card penaltyCard)
        {
            const int PenaltyCardCount = 4;

            if (dealtCard is RegularCard regularCard)
                return regularCard.IsEqual(penaltyCard) ? PenaltyCardCount : 0;
            else
                return 0;
        }

        public static ActionInstruction SpecialCardAction(Card currentCard)
        {
            switch (currentCard)
            {
                case SpecialCard specialCard:
                    if (specialCard.CardType.Equals(SpecialCardType.Skip))
                        return ActionInstruction.SkipTurn;

                    /*
                    else if (specialCard.CardType.Equals(SpecialCardType.ChangeOrder))
                        return ActionInstruction.ChangeOrder;
                    */

                    else if (specialCard.CardType.Equals(SpecialCardType.DrawFour))
                        return ActionInstruction.DrawFour;

                    else
                        return ActionInstruction.DrawTwo;
                default:
                    return ActionInstruction.DoNothing;
            }
        }

        private static bool SameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            bool isFirstCardRed = (firstRegularCard.Suit.Equals(Suits.Hearts) || firstRegularCard.Suit.Equals(Suits.Diamonds));
            bool isSecondCardRed = (secondRegularCard.Suit.Equals(Suits.Hearts) || secondRegularCard.Suit.Equals(Suits.Diamonds));

            return isFirstCardRed != isSecondCardRed;
        }

        public static bool PotentialPlayerAction(Card? lastPlayedCard, bool computerSkipped, Deck gameDeck, Computer computer, Card penaltyCard)
        {
            int actualPickupCount = 0;

            if (lastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(lastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        break;

                    /*
                                        case ActionInstruction.ChangeOrder:
                                            MainGame.GameModeChoice = MainGame.GameModeChoice == GameMode.Ascending ? GameMode.Descending : GameMode.Ascending;
                                            MainGame.Output.WriteLine($"\nThe game mode is now {MainGame.GameModeChoice}");

                                            MainGame.IsPlayerTurn = false;
                                            break;
                    */

                    case ActionInstruction.SkipTurn:
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        for (int i = 0; i < 4; i++)
                        {
                            Card? drawnCard = gameDeck.DealCard();
                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);

                                    for (int j = 0; j < awardedPenalty; j++)
                                    {
                                        Card? penaltyDrawnCard = gameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            computer.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                computer.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        for (int i = 0; i < 2; i++)
                        {
                            Card? drawnCard = gameDeck.DealCard();
                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);

                                    for (int j = 0; j < awardedPenalty; j++)
                                    {
                                        Card? penaltyDrawnCard = gameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            computer.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                computer.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        computerSkipped = true;
                        break;

                    default:
                        break;
                }
                ;
            }
            return computerSkipped;
        }
        public static bool PotentialComputerAction(Card? lastPlayedCard, bool playerSkipped, Deck gameDeck, Player player, Card penaltyCard)
        {
            int actualPickupCount = 0;

            if (lastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(lastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        break;

                    /*
                        case ActionInstruction.ChangeOrder:
                            MainGame.GameModeChoice = MainGame.GameModeChoice == GameMode.Ascending ? GameMode.Descending : GameMode.Ascending;
                            MainGame.Output.WriteLine("\n---------------------------------------------------------------------");
                            MainGame.Output.WriteLine($"\nThe game mode is now {MainGame.GameModeChoice}");

                            MainGame.IsPlayerTurn = true;
                            break;
                    */

                    case ActionInstruction.SkipTurn:
                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        for (int i = 0; i < 4; i++)
                        {
                            Card? drawnCard = gameDeck.DealCard();
                            
                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);

                                    for (int j = 0; j < awardedPenalty; j++)
                                    {
                                        Card? penaltyDrawnCard = gameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            player.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                player.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        for (int i = 0; i < 2; i++)
                        {
                            Card? drawnCard = gameDeck.DealCard();

                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    int awardedPenalty = GetPenaltyCount(drawnCard, penaltyCard);
                                    
                                    for (int j = 0; j < awardedPenalty; j++)
                                    {
                                        Card? penaltyDrawnCard = gameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            player.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                player.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        playerSkipped = true;
                        break;

                    default:
                        break;
                }
                ;
            }
            return playerSkipped;
        }

        public static Card? PreventInitalSpecialCard(Card logicCard, Deck gameDeck)
        {
            if (logicCard is not null)
            {
                while (logicCard is SpecialCard)
                {
                    List<Card> temporarySpecialCards = [];
                    temporarySpecialCards.Add(logicCard);

                    if (gameDeck.Length() > 0)
                        logicCard = gameDeck.DealCard()!;

                    gameDeck.AddRange(temporarySpecialCards);
                    gameDeck.InHouseShuffle();
                }

                return logicCard;
            }
            else
            {
                return null;
            }

        }
    }
}