using MarketSphere.Models.Gifts;

namespace MarketSphere.Services
{
    public class GiftsAdviceHandler
    {
        private GiftAdviceFlowService _adviceFlowService;

        public GiftsAdviceHandler(GiftAdviceFlowService adviceFlowService)
        {
            _adviceFlowService = adviceFlowService;
        }

        public void StartParse(GiftsParseParameters parseParameters)
        {
            _adviceFlowService.StartParse(parseParameters.Url);
        }

        public void StartAdvice(AdviceRequest request)
        {
            _adviceFlowService.StartAdvice(request.Text, request.Line);
        }
    }
}
