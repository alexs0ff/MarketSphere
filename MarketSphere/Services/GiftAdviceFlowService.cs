using MarketSphere.Models;
using MarketSphere.Models.Gifts;

namespace MarketSphere.Services
{
    public class GiftAdviceFlowService
    {
        private GiftAdviceFlow _flow = new GiftAdviceFlow();

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public GiftAdviceFlowService()
        {
            _flow.Step = GiftAdviceStep.New;
        }

        public GiftAdviceFlow GetFlow()
        {
            return _flow;
        }

        public void StartParse(string url)
        {
            _flow = new GiftAdviceFlow();
            _flow.UrlToParse = url;
            _flow.Step = GiftAdviceStep.ParseUrl;
        }

        public void CleanProgress()
        {
            _flow.CurrentProgress = 0;
        }

        public void StartProgress()
        {
            _flow.CurrentProgress = 1;
        }

        public void EndProgress()
        {
            _flow.CurrentProgress = 100;
        }
       
        public void Progress(int total, int current)
        {
            _flow.CurrentProgress = (int)((current*1.0)/total * 100.00);
        }

        public void SetClassified(IReadOnlyCollection<ClassifiedGift> processedGifts)
        {
            _flow.ClassifiedGifts = processedGifts;
        }

        public void StartClassification(IReadOnlyCollection<GiftItem> gifts)
        {
            _flow.Gifts = gifts;
            _flow.Step = GiftAdviceStep.Classifier;
        }

        public void ClassifierCompleted()
        {
            _flow.Step = GiftAdviceStep.ClassifierCompleted;
        }

        public void StartAdvice(string text, string line)
        {
            if (_flow.Step != GiftAdviceStep.ClassifierCompleted && _flow.Step != GiftAdviceStep.AdviceReady)
            {
                return;
            }

            _flow.TextToAdvice = text;
            _flow.LineToAdvice = line;
            _flow.Step = GiftAdviceStep.Advice;
            _flow.Advice = null;
        }

        public void SetAdvice(IReadOnlyCollection<string> hobbies, string age, IReadOnlyCollection<GiftItem> gifts)
        {
            if (_flow.Step == GiftAdviceStep.ParseUrl)
            {
                return;
            }

            _flow.Advice = new AdviceResult { Gifts = gifts, SuggestedAge = age, SuggestedHobbies = hobbies,Id = Guid.NewGuid()};
            _flow.Step = GiftAdviceStep.AdviceReady;
        }

        public void SetMaxPrice(int? maxPrice)
        {
            if (_flow.Step == GiftAdviceStep.ParseUrl)
            {
                return;
            }

            if (maxPrice is null || _flow.CurrentMaxPrice.HasValue)
            {
                return;
            }
            if (maxPrice > 100)
            {
                _flow.CurrentMaxPrice = maxPrice;
            }
        }

        public int? GetMaxPrice()
        {
            return _flow.CurrentMaxPrice;
        }

        public void SetPersonAge(string currentPersonAge)
        {
            if (_flow.Step == GiftAdviceStep.ParseUrl)
            {
                return;
            }
            _flow.CurrentPersonAge= currentPersonAge;
        }

        public string? GePersonAge()
        {
            return _flow.CurrentPersonAge;
        }
    }
}
