using System.Runtime.Serialization;
using MarketSphere.Models.Gifts;

namespace MarketSphere.Models
{
    public class GiftAdviceFlow
    {
        public GiftAdviceStep Step { get; set; }
        internal string? UrlToParse { get; set; }
        
        internal string? TextToAdvice { get; set; }

        internal string? LineToAdvice { get; set; }

        public int CurrentProgress { get; set; }

        public int? CurrentMaxPrice { get; set; }
        public string? CurrentPersonAge { get; set; }

        public IReadOnlyCollection<GiftItem>? Gifts { get; set; }
        
        public IReadOnlyCollection<ClassifiedGift>? ClassifiedGifts { get; set; }
        
        public AdviceResult? Advice { get; set; }
    }
}
