using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireUno
{
    public class ComputerTurnHandler(Computer computer, Deck deck, GameDifficulty currentDifficulty)
    {
        private readonly Computer _computer = computer;
        private readonly Deck _deck = deck;
        private readonly GameDifficulty _gameDifficulty = currentDifficulty;

        public (string message, Card? playedCard) HandleTurn(ref Card logicCard, ref Card visualCard, Card penaltyCard, int opponentHandSize, GameMode currentGameMode, bool suitEnforcement)
        {
            Card? potentialComputerPlay = _computer.MakeMove(logicCard, opponentHandSize, _deck.Length(), _gameDifficulty, currentGameMode, suitEnforcement);

            if (potentialComputerPlay is null)
            {
                if (_deck.Length() > 0 || _deck.Length() == 0 && !_deck.DeckReshuffled)
                {
                    Card card = _deck.DealCard()!;
                    _computer.PickupCard(card);

                    int computerPotentialPenaltyCount = GameMethods.GetPenaltyCount(card, penaltyCard);

                    if (computerPotentialPenaltyCount > 0)
                    {
                        int actualPickupCount = 0;

                        for (int i = 0; i < computerPotentialPenaltyCount; i++)
                        {
                            Card? addtionalPenaltyCard = _deck.DealCard();

                            if (addtionalPenaltyCard is not null)
                            {
                                _computer.PickupCard(addtionalPenaltyCard);
                                actualPickupCount++;
                            }
                        }
                        return ($"The Computer decided to pick up and found the {penaltyCard}! It picked up {actualPickupCount} additional cards!", null);
                    }

                    else
                        return ("The Computer decided to pick up!", null);
                }

                else if (_deck.Length() == 0 && _deck.DeckReshuffled)
                    return ("The Computer decided to pass!", null);
            }

            else
            {
                visualCard = potentialComputerPlay;

                if (potentialComputerPlay is RegularCard)
                    logicCard = potentialComputerPlay;

                _computer.PlayCard(potentialComputerPlay);
                _deck.AddToDiscardPile(potentialComputerPlay);


                if (potentialComputerPlay is SpecialCard specialCard && specialCard.CardType == SpecialCardType.Skip)
                    return ($"The Computer played: {potentialComputerPlay} and skipped you!", potentialComputerPlay);

                else if ((potentialComputerPlay is SpecialCard specialCard2 && specialCard2.CardType == SpecialCardType.DrawFour) || (potentialComputerPlay is SpecialCard specialCard3 && specialCard3.CardType == SpecialCardType.DrawTwo))
                    return ($"The Computer played: {potentialComputerPlay}, so you had to draw!", potentialComputerPlay);

                return ($"The Computer decided to play: {potentialComputerPlay}!", potentialComputerPlay);
            }
            
            return ("The Computer got scared...", null);
        }
    }
}
