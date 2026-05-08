namespace SolitaireUno
{
    public class RegularCard(Suits suit, Values value) : Card
    {
        public Suits Suit { get; } = suit;
        public Values Value { get; } = value;

        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }

        public bool IsEqual(Card otherCard)
        {
            return otherCard is not null and RegularCard regularCard 
                && this.Value == regularCard.Value && this.Suit == regularCard.Suit;
        }
    }
}
