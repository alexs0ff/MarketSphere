namespace MarketSphere.Models.Gifts
{
    public class ClassifiedGift
    {
        public GiftItem Gift { get; set; }

        public string Age { get; set; }

        public IReadOnlyCollection<string> Hobbies { get; set; }
    }
}
