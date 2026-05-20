using System;
using System.Collections.Generic;
using System.Linq;

namespace SolitaireUno
{
    /// <summary>
    /// Card validation helpers (play validity, wrap-around, suit checks).
    /// </summary>
    internal static class CardValidation
    {
        /// <summary>
        /// Determines whether a potential play is valid given the current table card, game mode, and suit enforcement.
        /// </summary>
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameMode gameMode, bool suitEnforcement)
        {
            if (potentialPlay is RegularCard firstRegularCard && logicCardShown is RegularCard secondRegularCard)
            {
                bool isValidSequence = gameMode == GameMode.Descending
                    ? IsValidDescending(potentialPlay, logicCardShown)
                    : IsValidAscending(potentialPlay, logicCardShown);

                if (!isValidSequence)
                    return false;

                return suitEnforcement ? SameColor(firstRegularCard, secondRegularCard) : true;
            }

            return IsSpecialCard(potentialPlay);
        }

        /// <summary>
        /// Valid when the potential play value is exactly one less than the shown card, or a wrap-around case.
        /// </summary>
        private static bool IsValidDescending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value - 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Descending);
        }

        /// <summary>
        /// Valid when the potential play value is exactly one greater than the shown card, or a wrap-around case.
        /// </summary>
        private static bool IsValidAscending(Card potentialPlay, Card currentlyShown)
        {
            if (potentialPlay is RegularCard potentialCard && currentlyShown is RegularCard currentCard)
                if ((int)potentialCard.Value == (int)currentCard.Value + 1)
                    return true;

            return IsWrapAround(potentialPlay, currentlyShown, GameMode.Ascending);
        }

        /// <summary>
        /// Handles Ace/King wrap-around depending on game mode.
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

        public static bool IsSpecialCard(Card potentialPlay) => potentialPlay is SpecialCard;

        /// <summary>
        /// Returns true when the two regular cards are of the same color grouping (red vs black).
        /// </summary>
        private static bool SameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            bool isFirstCardRed = (firstRegularCard.Suit == Suits.Hearts || firstRegularCard.Suit == Suits.Diamonds);
            bool isSecondCardRed = (secondRegularCard.Suit == Suits.Hearts || secondRegularCard.Suit == Suits.Diamonds);

            return isFirstCardRed == isSecondCardRed;
        }
    }
}
