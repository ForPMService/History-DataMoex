using History_DataMoex.Options;
using History_DataMoex.Clients;
using History_DataMoex.Parsing;
using History_DataMoex.DataTransfers;

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
app.MapGet("/research/supercandles-tradestat-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/tradestats/SBER.json";
    List<SuperCandlesTradeStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesTradeStats5m(url,
        new Dictionary<string, string>
        {
            [ "from"]= "2026-04-26" ,
            [ "till"]= "2026-04-30"
            
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesTradeStats5mDTO);
});
//app.MapGet("/research/supercandles-orderstats-10", async (MoexHttpAlgClient moexHttpAlgClient) => {
//    string response = await moexHttpAlgClient.GetRaws("/datashop/algopack/eq/orderstats/SBER.json?from=2026-04-30&till=2026-04-30&limit=500");
//    return Results.Content(response, "application/json");
//});
//app.MapGet("/research/supercandles-obstats-10", async (MoexHttpAlgClient moexHttpAlgClient) => {
//    string response = await moexHttpAlgClient.GetRaws("/datashop/algopack/eq/obstats/SBER.json?from=2026-04-30&till=2026-04-30&limit=500");
//    return Results.Content(response, "application/json");
//});
app.MapGet("/research/candles-1", async (MoexHttpAlgClient moexHttpAlgClient) => {
    string url = "/engines/stock/markets/shares/boards/tqbr/securities/SBER/candles.json";
    List<CandlesDTO> response = await moexHttpAlgClient.GetCandles(url,
        new Dictionary<string, string>
        {
            [ "interval"]= "1" ,
            [ "from"]= "2026-04-26" ,
            [ "till"]= "2026-05-01",
            [ "start"]= "1000"
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
});
//app.MapGet("/research/candles-1-page-2", async (MoexHttpAlgClient moexHttpAlgClient) =>
//{
//    string response = await moexHttpAlgClient.GetRaws(
//        "/engines/stock/markets/shares/boards/tqbr/securities/SBER/candles.json" +
//        "?interval=1&from=2026-04-26&till=2026-05-01&start=500");

//    return Results.Content(response, "application/json");
//});

app.UseHttpsRedirection();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
