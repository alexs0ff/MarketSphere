using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MarketSphere.Models.Gifts
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum GiftAdviceStep
    {
        [EnumMember(Value = "new")]
        New,
        [EnumMember(Value = "parseUrl")]
        ParseUrl,
        [EnumMember(Value = "classifier")]
        Classifier,
        [EnumMember(Value = "classifierCompleted")]
        ClassifierCompleted,
        [EnumMember(Value = "advice")]
        Advice,
        [EnumMember(Value = "adviceReady")]
        AdviceReady
    }
}
