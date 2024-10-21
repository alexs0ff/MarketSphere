namespace MarketSphere.Models.Gifts
{
    public class AdviceResult
    {
        public Guid Id { get; set; }

        public IReadOnlyCollection<string> SuggestedHobbies { get; set; }
        
        public string SuggestedAge { get; set; }
        
        public IReadOnlyCollection<GiftItem> Gifts { get; set; }
    }
}
