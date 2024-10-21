using MarketSphere.Models.Gifts;
using System.Text.Json;
using System.Threading;

namespace MarketSphere.Services;

public class GiftsShopScrapper
{
    private readonly ILogger<GiftsShopScrapper> _logger;

    private readonly IWebHostEnvironment _webHostEnvironment;

    public GiftsShopScrapper(ILogger<GiftsShopScrapper> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IReadOnlyCollection<GiftItem>> Search(string url, CancellationToken cancellationToken)
    {
        try
        {
            var data = await Parse(cancellationToken);

            return data.Gifts.Select(s=>new GiftItem{ Url = s.Url,Description = s.Description,Name = s.Name,Price = s.Price, Meta = new GiftItemMeta
            {
                Age = s.Age,
                Hobbies = s.Hobbies
            }}).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error parse {Exception}",ex.Message);
        }


        var result = new List<GiftItem>();

        result.AddRange(new[]
        {
            new GiftItem{Name = "Детский конструктор \"Шарики\"",Description = "Этот подарок подойдет вашим детям, если они увлекаются конструированием из разноцветных кубиков, а также сбором интересных пазлов из подручных средств.",Price = 600,Url = "http://url1"},
            new GiftItem{Name = "Мастер-класс по вязанию",Description = "Мечтаете связать уникальный джемпер, который подчеркнет вашу индивидуальность и будет согревать вас этой зимой? Наш мастер-класс по вязанию джемпера — это идеальная возможность погрузиться в мир творчества и моды!Что вас ждет:\r\nПодробные пошаговые инструкции — даже если вы никогда не держали спицы в руках, вы с легкостью освоите все техники.\r\nАвторская схема и рекомендации по выбору пряжи и инструментов.\r\nПрактика на каждом этапе: от набора петель до создания идеального силуэта.\r\nЛайфхаки от профессионала, которые помогут вам вязать быстрее и аккуратнее.",Price = 3000,Url = "http://url2"},
            new GiftItem{Name = "Свечи ручной работы \"Снежная ягода\"",Description = "Набор ароматических свечей с запахом лесных ягод и лёгкими зимними нотами. Каждая свеча создана вручную, что придаёт ей особое тепло и индивидуальность. Идеальный подарок для уюта в доме.",Price = 1000,Url = "http://url3"},
            new GiftItem{Name = "Чайный набор \"Тепло и уют\"",Description = "Набор из глиняного чайника и чашек, выполненных в технике ручной лепки. Идеальный подарок для любителей чаепитий и поклонников восточной культуры",Price = 900,Url = "http://url4"}
        });

        return result;
    }

    private async Task<GiftData> Parse(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_webHostEnvironment.WebRootPath, "data\\gifts.json");
        var text = await File.ReadAllTextAsync(path, cancellationToken);

        GiftData? data = JsonSerializer.Deserialize<GiftData>(text, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (data?.Gifts?.Any() == true)
        {
            return data;
        }

        throw new Exception("Cannot find any gift data");
    }
   
}

internal class GiftData
{
    public GiftStoreItem[] Gifts { get; set; }
}

internal class GiftStoreItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string Age { get; set; }
    public string Url { get; set; }
    public string[]? Hobbies { get; set; }
}