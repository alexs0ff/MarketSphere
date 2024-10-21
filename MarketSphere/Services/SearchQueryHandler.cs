using MarketSphere.Models;

namespace MarketSphere.Services;

public sealed class SearchQueryHandler
{
    private readonly GptClient _gptClient;
    private readonly CitilinkShopScraper _citilinkShopScraper;

    public SearchQueryHandler(GptClient gptClient, CitilinkShopScraper citilinkShopScraper)
    {
        _gptClient = gptClient;
        _citilinkShopScraper = citilinkShopScraper;
    }

    public async Task<SearchResult> Search(string query, CancellationToken cancellationToken)
    {
        EquipmentQuery equipment = await _gptClient.GetEquipment(query);
        IReadOnlyCollection<ProductItem> items = await _citilinkShopScraper.Search(equipment.Subject, equipment.Rubles);

        return new SearchResult
        {
            Items = items
        };
        /*
        return new SearchResult
        {
            Items = new List<ProductItem>
            {
                new ProductItem { Img = "https://cdn.citilink.ru/MlBXaS-JKVNWw5jWSQnKMPcCl0GjIPTN7f-sI0QDg-g/resizing_type:fit/gravity:sm/width:220/height:220/plain/product-images/29f7f5f0-e92f-471a-a2b0-b111ea8bae78.jpg", Name = "Видеокарта Palit NVIDIA GeForce RTX 4060 RTX4060 DUAL OC", Price = "38 990", Url = "https://www.citilink.ru/product/videokarta-palit-nvidia-geforce-rtx-4060-rtx4060-dual-oc-8gb-dual-gddr-1942195/" },
                new ProductItem { Img = "https://cdn.citilink.ru/qWJbPQYe1OMmfOVsvFr2Ce7YJEZNcBFwE5yY0oybe8g/resizing_type:fit/gravity:sm/width:220/height:220/plain/product-images/411d1ae3-7e79-4dc9-abbc-dcc786de8863.jpg", Name = "Видеокарта ASUS NVIDIA GeForce RTX 4080 Super ROG-STRIX-RTX4080S-16G-WHITE", Price = "193 220", Url = "https://www.citilink.ru/product/videokarta-asus-pci-e-4-0-rog-strix-rtx4080s-16g-white-nv-rtx4080-supe-2012581/" },
                new ProductItem { Img = "https://cdn.citilink.ru/79-s88gCEb6jLxL03Sdk9yn1ygL36wDs68K6p_LYdx8/resizing_type:fit/gravity:sm/width:220/height:220/plain/product-images/3f30c572-2917-4d37-9b41-7e2e00617fd4.jpg", Name = "Видеокарта Palit NVIDIA GeForce RTX 3060 PA-RTX3060 DUAL 12G", Price = "35 400", Url = "https://www.citilink.ru/product/videokarta-palit-nvidia-geforce-rtx-3060-pa-rtx3060-dual-12g-12gb-dual-1469005/" },
            }
        };*/
    }
}