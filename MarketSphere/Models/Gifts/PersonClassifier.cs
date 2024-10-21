using MarketSphere.Services;
using System.Threading;
using System.Xml;

namespace MarketSphere.Models.Gifts;

public class PersonClassifier
{
    private readonly GptClient _client;

    private readonly ILogger<PersonClassifier> _logger;

    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public PersonClassifier(GptClient client, ILogger<PersonClassifier> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<string>> ClassifyHobbies(string personDescription, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Определение  потребностей:{personDescription}");
        
        var hobbies = await _client.ClassifyHobbies(personDescription, cancellationToken);

        return hobbies;
    }

    public async Task<string> ClassifyAge(string personDescription, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Определение  возраста :{personDescription}");
        
        var age = await _client.ClassifyAgeForPerson(personDescription, cancellationToken);

        return age;
    }

    public async Task<int?> GetMaxPrice(string line, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Определение  максимальной цены:{line}");

        var maxRubles = await _client.GetMaxPrice(line, cancellationToken);

        return maxRubles;
    }
}