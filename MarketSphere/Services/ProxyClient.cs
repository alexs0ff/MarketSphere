namespace MarketSphere.Services;

public class ProxyClient
{
    private readonly HttpClient _httpClient;

    public ProxyClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetBinary(string url, CancellationToken cancellationToken)
    {
        return await _httpClient.GetByteArrayAsync(url,cancellationToken);
    }
}