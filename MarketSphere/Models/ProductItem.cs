namespace MarketSphere.Models;

public sealed record class ProductItem
{
    public string Name { get; set; }
    public string Price { get; set; }
    public string Img { get; set; }
    public string Url { get; set; }
    public bool? HasComments { get; set; }
}