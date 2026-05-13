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
    /// <summary>
    /// This class contains all the crucial methods of the SolitaireUno experience.
    /// </summary>
    public class GameMethods
    {
        /// <summary>
        /// This is the most important methods of the game. It takes a card, compares it with another, and determines whether it is
        ///     a valid play based on the game mode chosen, and whether suit enforcement is on.
        /// </summary>
        /// <param name="potentialPlay">Represents the card a player wants to play/put down. Compared with logicCardShown </param>
        /// <param name="logicCardShown">Represents the card that is currently face up on the table</param>
        /// <param name="gameMode">Represents the current mode of the game</param>
        /// <param name="suitEnforcement">Represents the suit enforcement mode of the game</param>
        /// <returns>True if the card a player wants to play is valid, False if it isn't </returns>
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameMode gameMode, bool suitEnforcement)
        {
            // Checks to see if the card a player wants to play and the card on the table are both regular cards.
            if (potentialPlay is RegularCard firstRegularCard && logicCardShown is RegularCard secondRegularCard)
            {
                // ... if so, we create an isValidSequence variable that stores if the 
                    // card is valid based on the IsValidAscending/Descending methods
                bool isValidSequence = gameMode == GameMode.Descending
                    ? IsValidDescending(potentialPlay, logicCardShown)
                    : IsValidAscending(potentialPlay, logicCardShown);

                // if the sequence is not valid, return False to ValidCard()
                if (!isValidSequence)
                {
                    return false;
                }

                // checks to see if suits are enforced, if so it calls SameColor(), if not and 
                    // we reached this point, the card is likely valid.
                return suitEnforcement ? SameColor(firstRegularCard, secondRegularCard) : true;
            }

            // if either potential play or the card on the table isn't a regular card,
            //      then we return the value of isSpecialCard(). 
            else
            {
                return IsSpecialCard(potentialPlay);
            }
        }


        /// <summary>
        /// This determines if the card is valid based on the descending game mode. 
        /// </summary>
        /// <param name="potentialPlay">The card a player wants to play. </param>
        /// <param name="currentlyShown">The card that's currently face up on the table.  </param>
        /// <returns>Returns True if the play is valid, False if it isn't</returns>
        private static bool IsValidDescending(Card potentialPlay, Card currentlyShown)
        { 
            // checks to see if cards compared are regular cards, if so, then we check to see if
            //      the value of the potential play is one less than the value of the card on the table
            //      if returns True if so.
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value - 1)
                    return true;

            // if we havent returned True, IsWrapAround() is called
            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Descending);
        }

        /// <summary>
        /// Works like IsValidDescending, but focuses on the ascending game mode. 
        /// </summary>
        /// <param name="potentialPlay">The card the player wants to play. </param>
        /// <param name="currentlyShown">The card that's currently on the table. </param>
        /// <returns>Returns True if the play is valid, False if it isn't</returns>
        private static bool IsValidAscending(Card potentialPlay, Card currentlyShown)
        {
            // checks to see if cards compared are regular cards, if so, then we check to see if
            //      the value of the potential play is one more than the value of the card on the table
            //      if returns True if so.
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value + 1)
                    return true;

            // if we havent returned True, IsWrapAround() is called
            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Ascending);
        }

        /// <summary>
        /// This method checks to see if we are in a wrap around case. A King on an Ace or an Ace on a King.  
        /// </summary>
        /// <param name="potentalPlay">The card a player wants to play. </param>
        /// <param name="currentlyShown">The card that is currently on the table. </param>
        /// <param name="gameMode"> The current mode of the game. </param>
        /// <returns></returns>
        private static bool IsWrapAround(Card potentalPlay, Card currentlyShown, GameMode gameMode)
        {
            // checks to see if cards compared are regular cards, if so, then we check to see if
            //      the mode is Descending, if so, we return whether the potential card's value is a King, and the current card is an ace. 
            //      But if it's not descending, then it must be ascending, and then we check to see if the card to be played is an Ace while the current card is a King. 
            if (potentalPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if (gameMode == GameMode.Descending)
                    return potentialCard.Value == Values.King && currentCard.Value == Values.Ace;
                else
                    return potentialCard.Value == Values.Ace && currentCard.Value == Values.King;
            
            // if one or both cards are not regular cards, then it/they must be special card(s). 
            else
                return false;
        }

        /// <summary>
        /// This simply checks to see if a card to be played is a special card. 
        /// </summary>
        /// <param name="potentialPlay">The card to be played. </param>
        /// <returns></returns>
        public static bool IsSpecialCard(Card potentialPlay) => potentialPlay is SpecialCard;

        /// <summary>
        /// This determines if a dealt card is the penalty card.  
        /// </summary>
        /// <param name="dealtCard">The card dealt. </param>
        /// <param name="penaltyCard">The assigned penalty card.  </param>
        /// <returns></returns>
        public static int GetPenaltyCount(Card dealtCard, Card penaltyCard)
        { 
            // Here is the set penalty count if the dealt card is the penalty card. 
            const int PenaltyCardCount = 4;

            // If the dealt card is a regular card, AKA, not a special card, we call the is equal method to see if it is the penalty card.
            //      If it is, set the penalty count to PenaltyCardCount. If it's not, set it to 0.  
            if (dealtCard is RegularCard regularCard)
                return regularCard.IsEqual(penaltyCard) ? PenaltyCardCount : 0;
            
            // Return zero because special cards don't have penalties. 
            else
                return 0;
        }
        

        /// <summary>
        ///   This method returns an enum depending on the current card, if a special card was placed down.  
        ///   For instance, if draw four was put down. Return the draw four instruction. 
        ///   
        ///     If a regular card was put down then it returns the do nothing instruction. 
        /// </summary>
        /// <param name="currentCard">The card currently on the table. </param>
        /// <returns>An ActionInstruction depending on the card</returns>
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


        /// <summary>
        /// This method takes two cards and compares them depending on their suits. This method is important for the suit enforcement feature. 
        /// </summary>
        /// <param name="firstRegularCard"></param>
        /// <param name="secondRegularCard"></param>
        /// <returns>True if both the first and second cards aren't red suits, False if they are</returns>
        private static bool SameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            // IsFirstCardRed and isSecondCardRed looks at the suit and see if it equals a red suit AKA, Heart or diamonds. 
            bool isFirstCardRed = (firstRegularCard.Suit.Equals(Suits.Hearts) || firstRegularCard.Suit.Equals(Suits.Diamonds));
            bool isSecondCardRed = (secondRegularCard.Suit.Equals(Suits.Hearts) || secondRegularCard.Suit.Equals(Suits.Diamonds));

            return isFirstCardRed != isSecondCardRed;
        }

        public static bool PotentialPlayerAction(Card? lastPlayedCard, bool computerSkipped, Deck gameDeck, Computer computer, Card penaltyCard)
        {
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
                        ProcessDraw(4, computer, gameDeck, penaltyCard);
                        
                        computerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        ProcessDraw(2, computer, gameDeck, penaltyCard);

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
                        ProcessDraw(4, player, gameDeck, penaltyCard);

                        playerSkipped = true;
                        break;

                    case ActionInstruction.DrawTwo:
                        ProcessDraw(2, player, gameDeck, penaltyCard);

                        playerSkipped = true;
                        break;

                    default:
                        break;
                }
                ;
            }
            return playerSkipped;
        }

        /// <summary>
        ///  This prevents the initial card that is dealt from being a special card. 
        /// </summary>
        /// <param name="logicCard">The card on the table. </param>
        /// <param name="gameDeck">The game deck being used. </param>
        /// <returns>The new logic card, or Null</returns>
        public static Card? PreventInitalSpecialCard(Card logicCard, Deck gameDeck)
        {
            // This checks to see that if the logic card even exists and begins a loop that creates an empty list and adds the logic card inside the list it.
            // Checks to see if the game deck still has cards. If it does, set the new logic card to another Dealt card.
            // Also adds the temporary special cards list to the game deck and reshuffles it. 
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

        private static void ProcessDraw(int drawAmount, Player unfortunateSoul, Deck gameDeck, Card penaltyCard)
        {
            for (int i = 0; i < drawAmount; i++)
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
                                unfortunateSoul.PickupCard(penaltyDrawnCard);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    unfortunateSoul.PickupCard(drawnCard);
                }
                else
                {
                    break;
                }
            }
        }
    }
}