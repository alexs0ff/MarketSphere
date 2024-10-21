using MarketSphere.Models;

namespace MarketSphere.Services;

public class SummarizerHandler
{
    private string _current = string.Empty;

    private string? _currentUrl = null;

    public SummarizerStartResult Start(string itemUrl)
    {
        _current = string.Empty;

        _currentUrl = itemUrl;

        return new SummarizerStartResult
        {
            Token = Guid.NewGuid()
        };
    }

    public string? PopCurrentUrl()
    {
        string? currentUrl = _currentUrl;

        _currentUrl = null;

        return currentUrl;

    }

    public void Append(string text)
    {
        _current += text;
    }

    public SummarizerResult GetCurrentOutput()
    {
        return new SummarizerResult
        {
            Text = _current
        };
    }
}