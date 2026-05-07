using History_DataMoex.Options;
using History_DataMoex.Clients;
using History_DataMoex.Parsing;
using History_DataMoex.DataTransfers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient<MoexHttpIssClient>();
builder.Services.AddHttpClient<MoexHttpAlgClient>();
builder.Services.AddHttpClient<MoexHttpCalendarClient>();
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

app.MapGet("/research/fo-tradestats-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/fo/tradestats/SiM6.json";

    List<SuperCandlesFuturesTradeStats5mDTO> response =
        await moexHttpAlgClient.GetSuperCandlesFuturesTradeStats5m(url,
            new Dictionary<string, string>
            {
                ["from"] = "2026-04-28",
                ["till"] = "2026-05-05"
            });

    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesTradeStats5mDTO);
});
app.MapGet("/research/fo-obstats-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/fo/obstats/SiM6.json";

    List<SuperCandlesFuturesOrderBookStats5mDTO> response =
        await moexHttpAlgClient.GetSuperCandlesFuturesOrderBookStats5m(
            url,
            new Dictionary<string, string>
            {
                ["from"] = "2026-04-28",
                ["till"] = "2026-04-30"
            });

    return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesOrderBookStats5mDTO);
});



// === FUTOI ===
app.MapGet("/research/futoi-10", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/analyticalproducts/futoi/securities/Si.json";

    List<FutoiDTO> response = await moexHttpAlgClient.GetFutoi(
        url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-05-03",
            ["till"] = "2026-05-03"
        });

    return Results.Json(response, AppJsonContext.Default.ListFutoiDTO);
});

// === HI2 ===
app.MapGet("/research/hi2-eq-raw", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/hi2/SBER.json";

    List<Hi2AssetDTO> response = await moexHttpAlgClient.GetHi2Asset5m(url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-05-03",
            ["till"] = "2026-05-03"
        }
        );
    return Results.Json (response, AppJsonContext.Default.ListHi2AssetDTO);
});

app.MapGet("/research/hi2-fo-raw", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/fo/hi2/SiM6.json";
    List<Hi2FuturesDTO> response = await moexHttpAlgClient.GetHi2Furures5m(url,
    new Dictionary<string, string>
    {
        ["from"] = "2026-04-30",
        ["till"] = "2026-05-04"
    });
    return Results.Json(response, AppJsonContext.Default.ListHi2FuturesDTO);
});

 // === Mega Alerts ===
app.MapGet("/research/megaalerts-assets", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/eq/alerts/SBER.json";

    List<MegaAlertsAssetsDTO> response = await moexHttpAlgClient.GetMegaAlerts(
        url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-04-28",
            ["till"] = "2026-04-30"
        });

    return Results.Json(response, AppJsonContext.Default.ListMegaAlertsAssetsDTO);
});

app.MapGet("/research/megaalerts-futures", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/fo/alerts/SiM6.json";

    List<MegaAlertsFuturesDTO> response = await moexHttpAlgClient.GetMegaAlertsFutures(
        url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-04-28",
            ["till"] = "2026-04-30"
        });

    return Results.Json(response, AppJsonContext.Default.ListMegaAlertsFuturesDTO);
});

app.MapGet("/research/alerts-fo", async (MoexHttpAlgClient moexHttpAlgClient) =>
{
    string url = "/datashop/algopack/fo/alerts/SiM6.json";

    string response = await moexHttpAlgClient.GetRaw(
        url,
        new Dictionary<string, string>
        {
            ["from"] = "2026-04-28",
            ["till"] = "2026-04-30"
        });

    return Results.Content(response, "application/json");
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

app.MapGet("/research/fo-candles-1", async (MoexHttpAlgClient moexHttpAlgClient) => {
    string url = "/engines/futures/markets/forts/boards/RFUD/securities/SiM6/candles.json";
    List<CandlesDTO> response = await moexHttpAlgClient.GetCandles(url,
        new Dictionary<string, string>
        {
            ["interval"] = "1",
            ["from"] = "2026-04-28",
            ["till"] = "2026-05-05"
        });
    return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
});

// === ISS Calendar — Discovery ===

app.MapGet("/calendar/offdays-all", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetOffDaysAll(), AppJsonContext.Default.CalendarOffDaysAllDTO));

app.MapGet("/calendar/stock-offdays", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetStockOffDays(), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

app.MapGet("/calendar/futures-offdays", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetFuturesOffDays(), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

app.MapGet("/calendar/stock-session", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetStockSession(), AppJsonContext.Default.ListCalendarStockSessionDTO));

app.MapGet("/calendar/stock-session-types", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetStockSessionTypes(), AppJsonContext.Default.ListCalendarSessionTypeDTO));

app.MapGet("/calendar/futures-session", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetFuturesSession(), AppJsonContext.Default.ListCalendarFuturesSessionDTO));

app.MapGet("/calendar/futures-session-types", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetFuturesSessionTypes(), AppJsonContext.Default.ListCalendarSessionTypeDTO));

app.MapGet("/calendar/forts-contracts", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetFortsContracts(), AppJsonContext.Default.ListCalendarFortsContractDTO));

app.MapGet("/calendar/options-series", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetOptionsSeries(), AppJsonContext.Default.ListCalendarOptionsSeriesDTO));

app.MapGet("/calendar/suspended-reasons", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetSuspendedReasons(), AppJsonContext.Default.ListCalendarSuspendedReasonDTO));

app.MapGet("/calendar/suspended", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetSuspended(), AppJsonContext.Default.ListCalendarSuspendedDTO));

app.MapGet("/calendar/security-attributes", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetSecurityAttributes(), AppJsonContext.Default.ListCalendarSecurityAttributeDTO));

app.MapGet("/calendar/security-changes", async (MoexHttpCalendarClient c) =>
    Results.Json(await c.GetSecurityChanges(), AppJsonContext.Default.ListCalendarSecurityChangeDTO));

app.UseHttpsRedirection();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
