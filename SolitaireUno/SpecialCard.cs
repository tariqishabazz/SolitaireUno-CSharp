namespace SolitaireUno
{
    public class SpecialCard(SpecialCardType specialCardType) : Card
    {
        public SpecialCardType CardType { get; set; } = specialCardType;
        public override string ToString()
        {
            return $"{CardType}";
        }
    }
}
