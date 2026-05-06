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

// === Фьючерсы ===
app.MapGet("/research/fo-tradestats-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/fo/tradestats/SiM5.json",
        new Dictionary<string, string> { ["from"] = "2026-04-28", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});
app.MapGet("/research/fo-obstats-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/fo/obstats/SiM5.json",
        new Dictionary<string, string> { ["from"] = "2026-04-28", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});



// === FUTOI ===
app.MapGet("/research/futoi-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/fo/futoi/SiM5.json",
        new Dictionary<string, string> { ["from"] = "2026-04-28", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});

// === HI2 ===
app.MapGet("/research/hi2-eq-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/eq/hi2/SBER.json",
        new Dictionary<string, string> { ["from"] = "2026-04-01", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});
app.MapGet("/research/hi2-fo-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/fo/hi2/SiM5.json",
        new Dictionary<string, string> { ["from"] = "2026-04-01", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});

// === Mega Alerts ===
app.MapGet("/research/alerts-raw", async (MoexHttpAlgClient c) =>
{
    string raw = await c.GetRaw("/datashop/algopack/eq/alerts/SBER.json",
        new Dictionary<string, string> { ["from"] = "2026-04-28", ["till"] = "2026-04-30" });
    return Results.Content(raw, "application/json");
});

app.MapGet("/research/supercandles-tradestat-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/tradestats/SMLT.json";
    List<SuperCandlesTradeStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesTradeStats5m(url,
        new Dictionary<string, string>
        {
            [ "from"]= "2026-04-08" ,
            [ "till"]= "2026-04-17"
            
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesTradeStats5mDTO);
});
app.MapGet("/research/supercandles-orderstats-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/orderstats/SMLT.json";
    List<SuperCandlesOrderStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesOrderStats5m(url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-04-08",
            ["till"] = "2026-04-17"
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderStats5mDTO);
});
app.MapGet("/research/supercandles-obstats-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/obstats/SMLT.json";
    List<SuperCandlesOrderBookStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesOrderBookStats5m(url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-04-08",
            ["till"] = "2026-04-17"
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderBookStats5mDTO);
});
app.MapGet("/research/candles-1", async (MoexHttpAlgClient moexHttpAlgClient) => {
    string url = "/engines/stock/markets/shares/boards/tqbr/securities/SMLT/candles.json";
    List<CandlesDTO> response = await moexHttpAlgClient.GetCandles(url,
        new Dictionary<string, string>
        {
            [ "interval"]= "1" ,
            [ "from"]= "2026-04-08" ,
            [ "till"]= "2026-04-17"
            
        }
        );
    return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
});


app.UseHttpsRedirection();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
