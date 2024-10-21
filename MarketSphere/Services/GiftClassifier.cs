using MarketSphere.Models.Gifts;

namespace MarketSphere.Services
{
    public class GiftClassifier
    {
        private readonly GptClient _client;

        public GiftClassifier(GptClient client)
        {
            _client = client;
        }

        public async Task<ClassifiedGift> Classify(GiftItem gift, CancellationToken cancellationToken)
        {
            var age = await _client.ClassifyAgeForGift(gift.Description,cancellationToken);
            var hobbies = await _client.ClassifyHobbies($"{gift.Name} {gift.Description}", cancellationToken);
            return new ClassifiedGift
            {
                Gift = gift,
                Age = age.Trim().ToLowerInvariant(),
                Hobbies = hobbies.Select(h => h.Trim().ToLowerInvariant()).ToList()
            };
        }
    }
}
