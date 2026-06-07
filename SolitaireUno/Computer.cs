// Computer.cs — AI player logic for selecting a move based on game settings.

namespace SolitaireUno
{
    /// <summary>
    /// Computer player AI responsible for choosing and playing cards.
    /// </summary>
    public class Computer : Player
    {
        /// <summary>
        /// Determines and returns a card the computer will play based on the current table card, opponent's hand size,
        /// current deck size and the provided game settings. Returns null when no valid move exists.
        /// </summary>
        /// <param name="logicCard">The current logic card on the table used for validation.</param>
        /// <param name="opponentHandSize">The number of cards in the opponent's hand.</param>
        /// <param name="currentDeckSize">The current number of cards remaining in the deck.</param>
        /// <param name="currentGameSettings">The game settings (mode, difficulty, suit enforcement, player count).</param>
        /// <returns>The chosen card to play, or null if no valid move exists.</returns>
        public Card? MakeMove(Card logicCard, int opponentHandSize, int currentDeckSize, GameSettings currentGameSettings)
        {
            List<Card> regularMoves = [];
            List<Card> specialMoves = [];

            foreach (Card potentialCard in Hand)
            {
                if (!CardValidation.ValidCard(potentialCard, logicCard, currentGameSettings))
                    continue;

                if (potentialCard is SpecialCard)
                    specialMoves.Add(potentialCard);

                else
                    regularMoves.Add(potentialCard);
            }

            int totalValidMoves = regularMoves.Count + specialMoves.Count;

            if (totalValidMoves == 0)
                return null;


            // ======= MAKES RANDOM EASY MOVE IN EASY MODE
            //          WHILE STILL CONSIDERING DECK SIZE  ====== //

            if (currentGameSettings.Difficulty is GameDifficulty.Easy)
            {
                if (currentDeckSize <= 15 && specialMoves.Count > 0)
                {
                    foreach (Card specialCard in specialMoves)
                    {
                        if (specialCard is SpecialCard special)
                        {
                            if (special.CardType is SpecialCardType.DrawTwo or SpecialCardType.DrawFour)
                                return special;  
                        }
                    }
                }

                int randomIndex = Random.Shared.Next(totalValidMoves);
                return randomIndex < regularMoves.Count ? regularMoves[randomIndex] : specialMoves[randomIndex - regularMoves.Count];
            }

            // ======= CALCULATE PANIC THRESHOLD BASED ON DIFFICULTY ====== //
            int panicThreshold = currentGameSettings.Difficulty == GameDifficulty.Hard ? 4 : 7;


            // ======== CHANGE IF/WHEN ADDING NEW SPECIAL CARDS ======== //
            // ============ IF DECK IS RUNNING LOW ========== //

            if (currentDeckSize <= 10 && specialMoves.Count > 0)
            {
                Card? fallbackSpecial = null;

                foreach(Card specialCard in specialMoves)
                {
                    if(specialCard is SpecialCard special)
                    {
                        if (special.CardType is SpecialCardType.DrawTwo or SpecialCardType.DrawFour)
                            return special;

                        else
                            fallbackSpecial = special;
                    }
                }

                if(fallbackSpecial is not null)
                    return fallbackSpecial;
            }


            // ======== IF OPPONENT'S HAND SIZE IS GETTING SMALLER ======= //

            if(opponentHandSize <= panicThreshold)
            {
                if (specialMoves.Count == 0)
                    if (regularMoves.Count > 0)
                        return regularMoves[Random.Shared.Next(regularMoves.Count)];
               
                if (totalValidMoves == 1 && specialMoves[0] is SpecialCard special && special.CardType == SpecialCardType.Skip)
                    return special;

                return specialMoves[Random.Shared.Next(specialMoves.Count)];
            }


            // ========= IF NONE OF ABOVE, MAKE A RANDOM REGULAR/SPECIAL MOVE ======== //

            if (regularMoves.Count > 0)
                return regularMoves[Random.Shared.Next(regularMoves.Count)];

            return specialMoves[Random.Shared.Next(specialMoves.Count)];

        }
    }
}