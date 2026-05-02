using History_DataMoex.Options;
using History_DataMoex.Clients;
using History_DataMoex.Parsing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient<MoexHttpIssClient>();
builder.Services.AddHttpClient<MoexHttpAlgClient>();
builder.Services.Configure<MoexIssOptions>(
    builder.Configuration.GetSection("MoexIss"));
builder.Services.Configure<MoexAlgOptions>(
    builder.Configuration.GetSection("MoexAlg"));


builder.Services.AddOpenApi();

var app = builder.Build();
app.MapGet("/GetStockMarkets", async (MoexHttpIssClient moexHttpIssClient) => {
    string url = "/engines/stock/markets/shares/boards/tqbr/securities.json";
    List<StockSecurityDTO> response = await moexHttpIssClient.GetInfoTradedStockAssets(url);
    return Results.Json(response, AppJsonContext.Default.ListStockSecurityDTO);
});
app.MapGet("/GetFuturesMarkets", async (MoexHttpIssClient moexHttpIssClient) => {
    string url = "/engines/futures/markets/forts/boards/RFUD/securities.json";
    List<FuturesSecurityDTO> response = await moexHttpIssClient.GetInfoTradedFuturesAssets(url);
    return Results.Json(response, AppJsonContext.Default.ListFuturesSecurityDTO);
});

app.UseHttpsRedirection();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
