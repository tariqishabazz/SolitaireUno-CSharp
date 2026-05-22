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
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameMode gameMode)
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

                return MainGame.SuitEnforcement ? NotSameColor(firstRegularCard, secondRegularCard) : true;
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

        private static bool NotSameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            bool isFirstCardRed = (firstRegularCard.Suit.Equals(Suits.Hearts) || firstRegularCard.Suit.Equals(Suits.Diamonds));
            bool isSecondCardRed = (secondRegularCard.Suit.Equals(Suits.Hearts) || secondRegularCard.Suit.Equals(Suits.Diamonds));

            return isFirstCardRed != isSecondCardRed;
        }

        public static void ShowHand()
        {
            MainGame.Output.WriteLine("\n---------------------------------------------------------------------");
            MainGame.Output.WriteLine("Your Hand: ");

            int index = 0;
            foreach (Card card in MainGame.player.Hand)
            {
                MainGame.Output.WriteLine($"   {index + 1}) {card}");
                index++;
            }

            MainGame.Output.WriteLine($"\nYou now have {MainGame.player.Hand.Count} cards");
            MainGame.Output.WriteLine($"The Computer now has {MainGame.computer.Hand.Count} cards");
        }

        public static Player? GameOverStats()
        {
            MainGame.Output.WriteLine("\n\n------------------------------------ GAME OVER! ------------------------------------");

            if (MainGame.computer.Hand.Count == 0)
            {
                MainGame.Output.WriteLine("\nYou Lose! You've been bested by the machine :(");
                return MainGame.computer;
            }
            else if (MainGame.player.Hand.Count == 0)
            {
                MainGame.Output.WriteLine("\nYou Win! You beat the computer! Congrats! :)");
                return MainGame.player;
            }
            return null;
        }

        public static bool PotentialPlayerAction()
        {
            bool computerSkipped = false;
            int actualPickupCount = 0;
            Card penaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            if (MainGame.LastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(MainGame.LastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        MainGame.IsPlayerTurn = false;
                        break;

                    /*
                                        case ActionInstruction.ChangeOrder:
                                            MainGame.GameModeChoice = MainGame.GameModeChoice == GameMode.Ascending ? GameMode.Descending : GameMode.Ascending;
                                            MainGame.Output.WriteLine($"\nThe game mode is now {MainGame.GameModeChoice}");

                                            MainGame.IsPlayerTurn = false;
                                            break;
                    */

                    case ActionInstruction.SkipTurn:
                        MainGame.Output.WriteLine($"\nthe Computer has been skipped!");
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        for (int i = 0; i < 4; i++)
                        {
                            Card? drawnCard = MainGame.GameDeck.DealCard();
                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    MainGame.Output.WriteLine($"\nthe Computer picked up the Queen of Spades during the Draw 4\n" +
                                        $"and must pick up addtional cards");

                                    for (int j = 0; j < GetPenaltyCount(drawnCard, penaltyCard); j++)
                                    {
                                        Card? penaltyDrawnCard = MainGame.GameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            MainGame.computer.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                MainGame.computer.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (actualPickupCount == 1)
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} card");

                        else if (actualPickupCount > 1)
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} cards");
                        
                        else
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} cards, because there aren't anymore!");
                        
                        break;

                    case ActionInstruction.DrawTwo:
                        for (int i = 0; i < 2; i++)
                        {
                            Card? drawnCard = MainGame.GameDeck.DealCard();
                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    MainGame.Output.WriteLine($"\nthe Computer picked up the Queen of Spades during the Draw 2\n" +
                                        $"and must pick up addtional cards");

                                    for (int j = 0; j < GetPenaltyCount(drawnCard, penaltyCard); j++)
                                    {
                                        Card? penaltyDrawnCard = MainGame.GameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            MainGame.computer.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                MainGame.computer.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (actualPickupCount == 1)
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} card");

                        else if (actualPickupCount > 1)
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} cards");
                        
                        else
                            MainGame.Output.WriteLine($"\nthe Computer just picked up {actualPickupCount} cards, because there aren't anymore!");


                        break;

                    default:
                        break;
                }
                ;
            }
            return computerSkipped;
        }
        public static bool PotentialComputerAction()
        {
            bool playerSkipped = false;
            int actualPickupCount = 0;
            Card penaltyCard = new RegularCard(Suits.Spades, Values.Queen);

            if (MainGame.LastPlayedCard is not null)
            {
                ActionInstruction message = SpecialCardAction(MainGame.LastPlayedCard);
                switch (message)
                {
                    case ActionInstruction.DoNothing:
                        MainGame.IsPlayerTurn = true;
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
                        MainGame.Output.WriteLine("\n---------------------------------------------------------------------");
                        MainGame.Output.WriteLine($"\nYou have been skipped!");

                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawFour:
                        for (int i = 0; i < 4; i++)
                        {
                            Card? drawnCard = MainGame.GameDeck.DealCard();

                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard)  > 0)
                                {
                                    MainGame.Output.WriteLine($"\nYou picked up the Queen of Spades during the Draw 4\n" +
                                        $"and must pick up addtional cards");

                                    for (int j = 0; j < GetPenaltyCount(drawnCard, penaltyCard); j++)
                                    {
                                        Card? penaltyDrawnCard = MainGame.GameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            MainGame.player.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                MainGame.player.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        MainGame.Output.WriteLine("\n---------------------------------------------------------------------");

                        if (actualPickupCount == 1)
                            MainGame.Output.WriteLine($"\nYou had to pick up {actualPickupCount} card");

                        else if (actualPickupCount > 1)
                            MainGame.Output.WriteLine($"\nYou had to pick up {actualPickupCount} cards");
                        
                        else
                            MainGame.Output.WriteLine($"\nYou just picked up {actualPickupCount} cards, because there aren't anymore!");

                        break;

                    case ActionInstruction.DrawTwo:
                        for (int i = 0; i < 2; i++)
                        {
                            Card? drawnCard = MainGame.GameDeck.DealCard();

                            if (drawnCard is not null)
                            {
                                if (GetPenaltyCount(drawnCard, penaltyCard) > 0)
                                {
                                    MainGame.Output.WriteLine($"\nYou picked up the Queen of Spades during the Draw 2\n" +
                                        $"and must pick up addtional cards");

                                    for (int j = 0; j < GetPenaltyCount(drawnCard, penaltyCard); j++)
                                    {
                                        Card? penaltyDrawnCard = MainGame.GameDeck.DealCard();
                                        if (penaltyDrawnCard is not null)
                                        {
                                            MainGame.player.PickupCard(penaltyDrawnCard);
                                            actualPickupCount++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                MainGame.player.PickupCard(drawnCard);
                                actualPickupCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        MainGame.Output.WriteLine("\n---------------------------------------------------------------------");

                        if (actualPickupCount == 1)
                            MainGame.Output.WriteLine($"\nYou had to pick up {actualPickupCount} card");

                        else if (actualPickupCount > 1)
                            MainGame.Output.WriteLine($"\nYou had to pick up {actualPickupCount} cards");
                        
                        else
                            MainGame.Output.WriteLine($"\nYou just picked up {actualPickupCount} cards, because there aren't anymore!");


                        break;

                    default:
                        break;
                }
                ;
            }
            return playerSkipped;
        }

        public static void ShowRoundSummary(Card visualCard, Card logicCard)
        {
            MainGame.Output.WriteLine("\n---------------------------------------------------------------------");
            MainGame.Output.WriteLine(visualCard is not SpecialCard
                                 ? $"\n                  Current Card = {visualCard}"
                                : $"\n                  Current Card = {visualCard}\n                  Last Logical Card = {logicCard}");

            MainGame.Output.WriteLine($"\n                  Order is {MainGame.GameModeChoice}");

            int deckLength = MainGame.GameDeck.Length();

            string message = deckLength switch
            {
                1 => $"Deck has {deckLength} card remaining!",
                > 0 => $"Deck has {deckLength} cards remaining!",
                _ when MainGame.GameDeck.deckReshuffled => "No more cards in the deck!",
                _ => "No more cards in the deck, but someone can pick up to reshuffle!"
            };

            if (MainGame.GameDeck.deckReshuffled && MainGame.GameDeck.Length() == 1)
                MainGame.Output.WriteLine($"\n\nDeck has been reshuffled with {MainGame.GameDeck.Length()} card remaining");

            else if (MainGame.GameDeck.deckReshuffled && MainGame.GameDeck.Length() > 0)
                MainGame.Output.WriteLine($"\n\nDeck has been reshuffled with {MainGame.GameDeck.Length()} cards remaining");

            else
                MainGame.Output.WriteLine($"\n\n{message}");
        }

        public static Card? PreventInitialSpecialCard(Card logicCard)
        {
            if (logicCard is not null)
            {
                while (logicCard is SpecialCard)
                {
                    List<Card> temporarySpecialCards = [];
                    temporarySpecialCards.Add(logicCard);

                    if (MainGame.GameDeck.Length() > 0)
                        logicCard = MainGame.GameDeck.DealCard()!;

                    MainGame.GameDeck.AddRange(temporarySpecialCards);
                    MainGame.GameDeck.InHouseShuffle();
                    
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