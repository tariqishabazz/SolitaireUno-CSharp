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
        /// Returns true when <paramref name="potentialPlay"/> is a legal play against <paramref name="logicCardShown"/> under <paramref name="currentGameSettings"/>.
        /// Regular cards must follow sequence and optional suit rules; special cards are always valid.
        /// </summary>
        public static bool ValidCard(Card potentialPlay, Card logicCardShown, GameSettings currentGameSettings)
        {
            if (potentialPlay is RegularCard firstRegularCard && logicCardShown is RegularCard secondRegularCard)
            {
                bool isValidSequence;

                if (currentGameSettings.Mode == GameMode.Both)
                {
                    isValidSequence = IsValidAscending(potentialPlay, logicCardShown) || IsValidDescending(potentialPlay, logicCardShown);
                }

                else
                {
                    isValidSequence = currentGameSettings.Mode == GameMode.Descending ? IsValidDescending(potentialPlay, logicCardShown) : IsValidAscending(potentialPlay, logicCardShown);
                }

                if (!isValidSequence)
                    return false;

                // If suits are enforced, the two regular cards must be of different color groups (red vs black).
                return !currentGameSettings.SuitsEnforced || NotSameColor(firstRegularCard, secondRegularCard);
            }

            // Special cards are valid plays regardless of regular-card sequencing.
            return IsSpecialCard(potentialPlay);
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
    }
}
