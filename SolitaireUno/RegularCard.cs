namespace SolitaireUno
{
    public class RegularCard(Suits suit, Values value) : Card
    {
        public Suits Suit { get; } = suit;
        public Values Value { get; } = value;

        public override string ToString()
        {

            string suitEmoji = Suit switch
            {
                Suits.Hearts => "❤️",
                Suits.Clubs => "♣",
                Suits.Diamonds => "🔷",
                Suits.Spades => "♠️",

                _ => ""
            };

            string rankEmoji = Value switch
            {
                Values.Queen => "👑",
                Values.King => "👑",
                
                _ => ""
            };

            return $"{Value} of {Suit} {rankEmoji}{suitEmoji}";
        }

        public bool IsEqual(Card otherCard)
        {
            return otherCard is not null and RegularCard regularCard 
                && this.Value == regularCard.Value && this.Suit == regularCard.Suit;
        }
    }
}
