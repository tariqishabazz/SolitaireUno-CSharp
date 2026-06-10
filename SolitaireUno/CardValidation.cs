// CardValidation.cs
// Helpers to validate whether a played card is legal given the current table card
// and game settings (mode and suit rules).

namespace SolitaireUno
{
    /// <summary>
    /// Validation helpers for card plays: sequence checks (ascending/descending), wrap-around rules, special-card detection and suit comparisons.
    /// </summary>
    public class CardValidation
    {
        /// <summary>
        /// Returns true when <paramref name="potentialPlay"/> is a legal play against <paramref name="logicCardShown"/> under <paramref name="gameSettings"/>.
        /// Regular cards must follow sequence and optional suit rules; special cards are always valid.
        /// </summary>
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameSettings gameSettings, bool isLeapFrog)
        {
            // If the potential play or the logic card shown are not regular cards,
            // check to see if the potential play is a special card. 
            if (potentialPlay is not RegularCard firstCard || logicCardShown is not RegularCard secondCard)
                return IsSpecialCard(potentialPlay);

            // If the first card and the second card have the same value,
            // check to see if we're enforcing suits. If we are, make sure
            // they're not the same color. If we aren't, it's still valid regardless. 
            if (IsSameValue(firstCard, secondCard))
                return gameSettings.SuitsEnforced ? NotSameColor(firstCard, secondCard) : true;


            int potentialCardValue = (int)firstCard.Value;
            int currentCardValue = (int)secondCard.Value;

            bool isValidSequence = false;


            // ================ IF MODE IS ASC/BOTH ================ //

            if (gameSettings.Mode is GameMode.Ascending or GameMode.Both)
            {
                int stepsForward = potentialCardValue > currentCardValue ?
                    potentialCardValue - currentCardValue : potentialCardValue - currentCardValue + 13;

                if (isLeapFrog || stepsForward is 1)
                    isValidSequence = true;
            }

            // ================= IF MODE IS DESC/BOTH ================= //

            if (!isValidSequence && (gameSettings.Mode is GameMode.Descending or GameMode.Both))
            {
                int stepsBackward = currentCardValue > potentialCardValue ?
                    currentCardValue - potentialCardValue : currentCardValue - potentialCardValue + 13;

                if (isLeapFrog || stepsBackward is 1)
                    isValidSequence = true;
            }


            if (!isValidSequence)
                return false;

            return gameSettings.SuitsEnforced ? NotSameColor(firstCard, secondCard) : true;
        }

        /// <summary>
        /// True when the potential play's value is exactly one less than the shown card, or when a descending wrap-around applies.
        /// </summary>
        private static bool IsValidDescending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value - 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Descending);
        }

        /// <summary>
        /// True when the potential play's value is exactly one greater than the shown card, or when an ascending wrap-around applies.
        /// </summary>
        private static bool IsValidAscending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value + 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Ascending);
        }

        /// <summary>
        /// Handles Ace/King wrap-around. For Descending mode, King follows Ace; for Ascending mode, Ace follows King.
        /// </summary>
        private static bool IsWrapAround(Card potentalPlay, Card currentlyShown, GameMode gameMode)
        {
            if (potentalPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
            {
                if (gameMode == GameMode.Descending)
                    return potentialCard.Value == Values.King && currentCard.Value == Values.Ace;

                return potentialCard.Value == Values.Ace && currentCard.Value == Values.King;
            }

            return false;
        }

        /// <summary>
        /// Returns true when the supplied card is a special card.
        /// </summary>
        public static bool IsSpecialCard(Card potentialPlay) => potentialPlay is SpecialCard;

        /// <summary>
        /// Returns true when the two regular cards are different color groups (red vs black).
        /// </summary>
        public static bool NotSameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            bool isFirstCardRed = (firstRegularCard.Suit == Suits.Hearts || firstRegularCard.Suit == Suits.Diamonds);
            bool isSecondCardRed = (secondRegularCard.Suit == Suits.Hearts || secondRegularCard.Suit == Suits.Diamonds);

            return isFirstCardRed != isSecondCardRed;
        }

        /// <summary>
        /// Returns true when the two regular cards are of the same suit.
        /// </summary>
        /// <param name="firstRegularCard"></param>
        /// <param name="secondRegularCard"></param>
        /// <returns></returns>
        public static bool IsSameSuit(RegularCard firstRegularCard, RegularCard secondRegularCard) => firstRegularCard.Suit == secondRegularCard.Suit;
                

        /// <summary>
        /// Returns true when the two regular cards are of the same value.
        /// </summary>
        /// <param name="firstRegularCard"></param>
        /// <param name="secondRegularCard"></param>
        /// <returns></returns>
        public static bool IsSameValue(RegularCard firstRegularCard, RegularCard secondRegularCard) => firstRegularCard.Value == secondRegularCard.Value;
        
    }
}