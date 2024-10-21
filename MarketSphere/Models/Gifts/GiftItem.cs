namespace MarketSphere.Models.Gifts
{
    public class GiftItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Price { get; set; }
        internal GiftItemMeta Meta { get; set; }
    }
}
