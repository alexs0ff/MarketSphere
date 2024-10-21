using MarketSphere.Models;
using MarketSphere.Models.Gifts;

namespace MarketSphere.Services
{
    public class GiftsAdviceHostedService : BackgroundService, IDisposable
    {
        private readonly ILogger<GiftsAdviceHostedService> _logger;
        private readonly GiftAdviceFlowService _flowService;
        private readonly IServiceProvider _services;

        public GiftsAdviceHostedService(ILogger<GiftsAdviceHostedService> logger, GiftAdviceFlowService flowService, IServiceProvider services)
        {
            _logger = logger;
            _flowService = flowService;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Gifts advice Service Hosted Service is working.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Flow(_flowService.GetFlow(), stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private Task Flow(GiftAdviceFlow flow, CancellationToken cancellationToken)
        {
            return flow switch
            {
                { Step: GiftAdviceStep.ParseUrl, UrlToParse: not null } => ProcessUrl(flow.UrlToParse,
                    cancellationToken),
                { Step: GiftAdviceStep.Classifier, Gifts: not null, ClassifiedGifts: null } => ProcessGifts(flow.Gifts,
                    cancellationToken),
                { Step: GiftAdviceStep.Advice, ClassifiedGifts: not null, TextToAdvice: not null,LineToAdvice: not null,Advice:null } => ProcessAdvice(flow.TextToAdvice, flow.LineToAdvice, flow.ClassifiedGifts, cancellationToken),
                _ => Task.CompletedTask
            };
        }

        private async Task ProcessUrl(string flowUrlToParse, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start parse gifts url {flowUrlToParse}");
            using var scope = _services.CreateScope();
            var scraper =
                scope.ServiceProvider
                    .GetRequiredService<GiftsShopScrapper>();
            IReadOnlyCollection<GiftItem> items = await scraper.Search(flowUrlToParse, cancellationToken);

            _flowService.StartClassification(items);
        }

        private async Task ProcessGifts(IReadOnlyCollection<GiftItem> gifts, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start classify gifts {gifts.Count}");
            using var scope = _services.CreateScope();
            var classifier =
                scope.ServiceProvider
                    .GetRequiredService<GiftClassifier>();

            _flowService.StartProgress();

            int index = 0;

            List<ClassifiedGift> result = new List<ClassifiedGift>();
            _flowService.SetClassified(result);

            foreach (var giftItem in gifts)
            {
                index++;

                ClassifiedGift classified;

                if (giftItem.Meta?.Hobbies?.Any() == true && giftItem.Meta?.Age?.Any() == true)
                {
                    classified = new ClassifiedGift
                    {
                        Gift = giftItem,
                        Hobbies = giftItem.Meta.Hobbies,
                        Age = giftItem.Meta.Age
                    };
                }
                else
                {
                    classified = await classifier.Classify(giftItem, cancellationToken);
                }
                result.Add(classified);
                
                _flowService.Progress(gifts.Count, index);
            }

            _flowService.EndProgress();
            
            _flowService.ClassifierCompleted();
        }

        private async Task ProcessAdvice(string personDescription, string lineToAdvice,IReadOnlyCollection<ClassifiedGift> classifiedGifts, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start classify person {personDescription}");
            using var scope = _services.CreateScope();
            var classifier =
                scope.ServiceProvider
                    .GetRequiredService<PersonClassifier>();

            var result = new ClassifiedPerson();//await classifier.Classify(personDescription, cancellationToken);

            var age = _flowService.GePersonAge();
            if (!string.IsNullOrEmpty(age))
            {
                result.Age = age;
            }
            else
            {
                result.Age = await classifier.ClassifyAge(personDescription, cancellationToken);
                _flowService.SetPersonAge(result.Age);
            }

            result.Hobbies = await classifier.ClassifyHobbies(personDescription, cancellationToken);

            int? maxPrice = _flowService.GetMaxPrice();

            if (!maxPrice.HasValue)
            {
                maxPrice = await classifier.GetMaxPrice(lineToAdvice, cancellationToken);
                _flowService.SetMaxPrice(maxPrice);
            }

            maxPrice = _flowService.GetMaxPrice();

            var gifts = new List<GiftItem>();

            foreach (var giftItem in classifiedGifts)
            {
                var currentHobbies = result.Hobbies.Select(s => s.ToLowerInvariant()).SelectMany(m=>m.Split(' '));
                bool hasHobbies = (giftItem.Hobbies.Select(s => s.ToLowerInvariant()).SelectMany(m=>m.Split(' ')).Intersect(currentHobbies).Any());
                if (hasHobbies && result.Age.Equals(giftItem.Age, StringComparison.OrdinalIgnoreCase))
                {
                    AddGiftByMaxPrice(maxPrice, giftItem, gifts);
                }

                if (gifts.Count > 2)
                {
                    break;
                }
            }

            if (!gifts.Any())
            {
                foreach (var giftItem in classifiedGifts)
                {
                    if (result.Age.Equals(giftItem.Age, StringComparison.OrdinalIgnoreCase))
                    {
                        AddGiftByMaxPrice(maxPrice, giftItem, gifts);
                    }

                    if (gifts.Count > 2)
                    {
                        break;
                    }
                }
            }

            _flowService.SetAdvice(result.Hobbies, result.Age, gifts);
        }

        private static void AddGiftByMaxPrice(int? maxPrice, ClassifiedGift giftItem, List<GiftItem> gifts)
        {
            if (maxPrice.HasValue)
            {
                if (giftItem.Gift.Price <= maxPrice.Value)
                {
                    gifts.Add(giftItem.Gift);
                }
            }
            else
            {
                gifts.Add(giftItem.Gift);
            }
        }
    }
}
