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
        public static bool ValidCard(Card potentialPlay, GameSettings gameSettings, GameState currentGameState)
        {
            if (potentialPlay is null || currentGameState.LogicCard is null)
                throw new ArgumentNullException("Cards cannot be null for validation.");

            if (IsSpecialCard(potentialPlay)) return true;

            if (potentialPlay is RegularCard potenchPlay && currentGameState.LogicCard is RegularCard logicCard)
            {
                bool suitEnforcementValidation = !gameSettings.SuitsEnforced || !SameColor(potenchPlay, logicCard);
                bool sameValueValidation = IsSameValue(potenchPlay, logicCard);

                if (sameValueValidation) return suitEnforcementValidation;

                int potentialCardValue = (int)potenchPlay.Value;
                int currentCardValue = (int)logicCard.Value;
                bool isValidSequence = false;

                if (currentGameState.LeapFrogMode)
                    isValidSequence = true;

                if (!isValidSequence && gameSettings.Mode is GameMode.Ascending or GameMode.Both)
                    isValidSequence = (potentialCardValue == currentCardValue + 1) || (potentialCardValue == 1 && currentCardValue == 13);

                if (!isValidSequence && (gameSettings.Mode is GameMode.Descending or GameMode.Both))
                    isValidSequence = (potentialCardValue == currentCardValue - 1) || (potentialCardValue == 13 && currentCardValue == 1);

                if (!isValidSequence) return false;

                return suitEnforcementValidation;
            }

            return false;
        }

        /// <summary>
        /// Returns true when the supplied card is a special card.
        /// </summary>
        public static bool IsSpecialCard(Card potentialPlay) => potentialPlay is SpecialCard;

        /// <summary>
        /// Returns true when the two regular cards are same color groups (hearts vs diamonds).
        /// </summary>
        public static bool SameColor(RegularCard firstRegularCard, RegularCard secondRegularCard)
        {
            bool isFirstCardRed = (firstRegularCard.Suit == Suits.Hearts || firstRegularCard.Suit == Suits.Diamonds);
            bool isSecondCardRed = (secondRegularCard.Suit == Suits.Hearts || secondRegularCard.Suit == Suits.Diamonds);

            return isFirstCardRed == isSecondCardRed;
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