using MarketSphere.Configs;
using MarketSphere.Models;
using MarketSphere.Models.Gifts;
using MarketSphere.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<SearchQueryHandler>();
builder.Services.AddScoped<CitilinkShopScraper>();
builder.Services.AddScoped<GiftsAdviceHandler>();
builder.Services.AddScoped<GiftsShopScrapper>();
builder.Services.AddScoped<GiftClassifier>();
builder.Services.AddScoped<PersonClassifier>();
builder.Services.AddScoped<GptClient>();
builder.Services.AddSingleton<SummarizerHandler>();
builder.Services.AddSingleton<GiftAdviceFlowService>();
builder.Services.AddHttpClient<ProxyClient>();
builder.Services.AddHostedService<SummarizerHostedService>();
builder.Services.AddHostedService<GiftsAdviceHostedService>();

builder.Services.Configure<OllamaConfig>(builder.Configuration.GetSection("Ollama"));


builder.Logging.ClearProviders();
builder.Services.AddLogging(cfg =>
{
    cfg.AddSimpleConsole(cs =>
    {
        cs.IncludeScopes = true;
        cs.ColorBehavior = LoggerColorBehavior.Enabled;
    });
});
//builder.Services.AddHttpLogging(c =>
//{

//});


var app = builder.Build();

//app.UseHttpLogging();

// Configure the HTTP request pipeline.
app.UseStaticFiles();

//app.UseHttpsRedirection();

app.MapGet("/search", async ([FromQuery] string query, SearchQueryHandler handler, CancellationToken cancellationToken) => await handler.Search(query, cancellationToken));

app.MapGet("/", async (context)=>
{ 
    context.Response.Redirect("/home.html");
   await Task.CompletedTask;
});

app.MapGet("/image", async ([FromQuery] string url, HttpContext context, ProxyClient proxyClient, CancellationToken cancellationToken) =>
{
    context.Response.Clear();
    context.Response.ContentType = "image/png";
    var data = await proxyClient.GetBinary(url, cancellationToken);
    await context.Response.Body.WriteAsync(data, 0, data.Length, cancellationToken);
});

app.MapGet("/start", ([FromQuery] string itemUrl, SummarizerHandler handler) => handler.Start(itemUrl));

app.MapGet("/summary", (SummarizerHandler handler) => handler.GetCurrentOutput());

app.MapGet("/giftsAdvice", (GiftAdviceFlowService service) => service.GetFlow());

app.MapPost("/giftsParse",
    ([FromBodyAttribute] GiftsParseParameters parameters, GiftsAdviceHandler service) =>
        service.StartParse(parameters));

app.MapPost("/advice",
    ([FromBodyAttribute] AdviceRequest parameters, GiftsAdviceHandler service) =>
        service.StartAdvice(parameters));

app.Run();

