using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    /// <summary>
    /// This classes handles all the card validation methods such as ValidCard(), SameColor(), etc.
    /// </summary>
    internal class CardValidation
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
                bool isValidSequence = gameMode == GameMode.Descending ? IsValidDescending(potentialPlay, logicCardShown) : IsValidAscending(potentialPlay, logicCardShown);

                // if the sequence is not valid, return False to ValidCard()
                if (!isValidSequence)
                    return false;

                // checks to see if suits are enforced, if so it calls SameColor(), if not and 
                // we reached this point, the card is likely valid.
                return suitEnforcement ? SameColor(firstRegularCard, secondRegularCard) : true;
            }

            // if either potential play or the card on the table isn't a regular card,
            //      then we return the value of isSpecialCard(). 
            else
                return IsSpecialCard(potentialPlay);
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


    }
}
