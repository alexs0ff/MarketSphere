namespace MarketSphere.Models
{
    public sealed record class SearchResult
    {
        public IReadOnlyCollection<ProductItem> Items { get; set; }
    }
}
