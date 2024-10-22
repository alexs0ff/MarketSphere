using OllamaSharp;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MarketSphere.Models;
using OllamaSharp.Models;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Rewrite;
using System.Data;
using MarketSphere.Configs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace MarketSphere.Services;

public sealed class GptClient
{
    private readonly ILogger<GptClient> _logger;
    private readonly Uri _ollamaUri;

    private const string LlamaModel = "llama3.2";
    private const string MistralModel = "mistral-small";

    //private const string SummarizePrompt = "Пожалуйста сократи текст: {0}";
    private const string SummarizePrompt = "Представь себя в роли эксперта по подбору компьютерных комплектующих и выведи только сокращенный текст на русском без своих комментариев: {0}";

    public GptClient(IOptions<OllamaConfig> config,ILogger<GptClient> logger)
    {
        _logger = logger;
        _ollamaUri = new Uri(config.Value.Url);
    }

    public async IAsyncEnumerable<string> Summarize(string text, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var ollama = new OllamaApiClient(_ollamaUri);
        ollama.SelectedModel = LlamaModel;
        var chat = new Chat(ollama);

        var toProcess = string.Format(SummarizePrompt, Environment.NewLine+text);
        _logger.LogInformation(toProcess);
        await foreach (var answerToken in chat.Send(toProcess, cancellationToken))
        {
            yield return answerToken;
        }
    }

    private const string EquipmentPrompt =
        "Скажи только предмет и его название, который нужно купить в этом утверждении: \"{0}\"";

    private const string PricePrompt =
        "Напиши только число, которое показывает ограничение по цене в утверждении, если ограничений нет, напиши \"нет\", используй только цифры:  \"{0}\"";
    public async Task<EquipmentQuery> GetEquipment(string query)
    {
        var ollama = new OllamaApiClient(_ollamaUri);

        ollama.SelectedModel = LlamaModel;
        var chat = new Chat(ollama);

        var answers = new StringBuilder();
        await foreach (var answerToken in chat.Send(string.Format(EquipmentPrompt, query)))
        {
            answers.Append(answerToken);
        }

        string subject = answers.ToString();

        var result = new EquipmentQuery
        {
            Subject = subject
        };

        int? rubles = null;

        answers.Clear();

        chat = new Chat(ollama);
        
        rubles = await GetMaxRubles(query, chat, CancellationToken.None);

        if (rubles.HasValue)
        {
            result.Rubles = rubles.Value;
        }

        _logger.LogInformation("Предмет: {Subject}, Цена:{Price}",result.Subject, result.Rubles);

        return result;
    }

    private async Task<int?> GetMaxRubles(string query, Chat chat, CancellationToken cancellationToken)
    {
        StringBuilder answers = new StringBuilder();
        int? rubles = null;
        var promt = string.Format(PricePrompt, query);
        _logger.LogInformation(promt);
        await foreach (var answerToken in chat.Send(promt, cancellationToken))
        {
            answers.Append(answerToken);
        }
        _logger.LogInformation(answers.ToString());

        var raw = answers.ToString().TrimEnd('.').Replace(" ", string.Empty);

        if (int.TryParse(raw, CultureInfo.InvariantCulture, out int v))
        {
            rubles = v;
        }

        return rubles;
    }

    private async Task<string> InnerAsk(string promt, string model, CancellationToken cancellationToken)
    {
        var ollama = new OllamaApiClient(_ollamaUri);
        ollama.SelectedModel = model;
        
        var chat = new Chat(ollama);
        chat.Options = new RequestOptions
        {
            Seed = 632
        };

        var sb = new StringBuilder();

        _logger.LogInformation(promt);
        await foreach (var answerToken in chat.Send(promt, cancellationToken))
        {
            sb.Append(answerToken);
        }
        _logger.LogInformation(sb.ToString());
        return sb.ToString();
    }
    
    private const string AgeGiftPrompt =
        "Забудь весь предыдущий текст. Проанализируй текст и напиши для какого возраста данный подарок подойдет. Используй варианты для выбора - это \"детский\" если описание релевантно для детей, \"взрослый\" если описание релевантно для взрослых увлечений. В ответе используй только одно слово: {0}{1}";
        //"Забудь весь предыдущий текст. Проанализируй текст и напиши для какого возраста данный подарок подойдет. Используй варианты для выбора - это \"детский\" , \"взрослый\". В ответе используй только одно слово: {0}{1}";
    
    //private const string AgeGiftPrompt = "Проанализируй текст и напиши для ребенка или взрослого подойдет описание подарка. Используй варианты для выбора - это \"детский\" если товар больше подходит для ребенка или \"взрослый\", если товар больше подходит для взрослого. В ответе используй только одно слово:: {0}{1}";
    public async Task<string> ClassifyAgeForGift(string text, CancellationToken cancellationToken)
    {
        var toProcess = string.Format(AgeGiftPrompt, Environment.NewLine, text);
        return await InnerAsk(toProcess, MistralModel, cancellationToken);
    }

    private const string AgePersonPrompt =
        "Проанализируй текст и напиши о ком идет речь в нем. Используй варианты для выбора - это \"ребенок\" если описывается ребенок или \"взрослый\", если описывается взрослый человек: {0}\"{1}\"";
    
    public async Task<string> ClassifyAgeForPerson(string text, CancellationToken cancellationToken)
    {
        var toProcess = string.Format(AgePersonPrompt, Environment.NewLine, text);
        var result = await InnerAsk(toProcess, MistralModel, cancellationToken);
        
        if (result.Contains("ребен", StringComparison.OrdinalIgnoreCase))
        {
            return "детский";
        }
        if (result.Contains("взросл", StringComparison.OrdinalIgnoreCase))
        {
            return "взрослый";
        }

        return string.Empty;
    }

    private const string HobbiesPrompt =
        "Забудь весь предыдущий текст. Проанализируй текст и напиши три варианта увлечений или хобби, для которых релевантно такое описание товара, выведи только названия через запятую, без пояснений: {0}{1}";

    public async Task<IReadOnlyCollection<string>> ClassifyHobbies(string text, CancellationToken cancellationToken)
    {
        var toProcess = string.Format(HobbiesPrompt, Environment.NewLine, text);
        var hobbies =  await InnerAsk(toProcess, MistralModel, cancellationToken);

        return hobbies.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<int?> GetMaxPrice(string line, CancellationToken cancellationToken)
    {
        var ollama = new OllamaApiClient(_ollamaUri);
        ollama.SelectedModel = LlamaModel;

        var chat = new Chat(ollama);
        chat.Options = new RequestOptions
        {
            Seed = 632
        };

        var rubles = await GetMaxRubles(line, chat, CancellationToken.None);

        return rubles;
    }
}