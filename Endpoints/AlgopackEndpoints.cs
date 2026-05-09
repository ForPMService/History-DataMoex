using History_DataMoex.Clients;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Contracts.Serialization;

namespace History_DataMoex.Endpoints
{
    /// <summary>
    /// Source endpoint-ы MOEX: в момент запроса идут в MOEX,
    /// парсят ответ и возвращают DTO MOEX.
    /// Это не ручки витрины для фронта; ручки витрины появятся позже
    /// и будут читать данные из PostgreSQL/ClickHouse.
    /// </summary>
    public static class AlgopackEndpoints
    {
        public static IEndpointRouteBuilder MapAlgopackEndpoints(this IEndpointRouteBuilder routes)
        {
            // === Фьючерсы ===
            routes.MapGet("/GetSuperCandlesFuturesTradeStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/tradestats/SiM6.json";

                List<SuperCandlesFuturesTradeStats5mDTO> response =
                    await moexHttpAlgClient.GetSuperCandlesFuturesTradeStats5m(url,
                        new Dictionary<string, string>
                        {
                            ["from"] = "2026-01-28",
                            ["till"] = "2026-05-05"
                        },
                        ct);

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesTradeStats5mDTO);
            });

            routes.MapGet("/GetSuperCandlesFuturesOrderBookStat", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/obstats/SiM6.json";

                List<SuperCandlesFuturesOrderBookStats5mDTO> response =
                    await moexHttpAlgClient.GetSuperCandlesFuturesOrderBookStats5m(
                        url,
                        new Dictionary<string, string>
                        {
                            ["from"] = "2026-01-28",
                            ["till"] = "2026-04-30"
                        },
                        ct);

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesOrderBookStats5mDTO);
            });


            // === FUTOI ===
            routes.MapGet("/GetFutoi", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/analyticalproducts/futoi/securities/Si.json";

                List<FutoiDTO> response = await moexHttpAlgClient.GetFutoi(
                    url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2026-05-03",
                        ["till"] = "2026-05-08"
                    },
                    ct);

                return Results.Json(response, AppJsonContext.Default.ListFutoiDTO);
            });

            // === HI2 ===
            routes.MapGet("/GetHi2Asset", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/hi2/SBER.json";

                List<Hi2AssetDTO> response = await moexHttpAlgClient.GetHi2Asset5m(url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2024-05-03",
                        ["till"] = "2026-05-03"
                    },
                    ct
                    );
                return Results.Json (response, AppJsonContext.Default.ListHi2AssetDTO);
            });

            routes.MapGet("/GetHi2Furure", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/hi2/SiM6.json";
                List<Hi2FuturesDTO> response = await moexHttpAlgClient.GetHi2Furures5m(url,
                new Dictionary<string, string>
                {
                    ["from"] = "2026-01-30",
                    ["till"] = "2026-05-04"
                },
                ct);
                return Results.Json(response, AppJsonContext.Default.ListHi2FuturesDTO);
            });

             // === Мега-оповещения ===
            routes.MapGet("/GetMegaAlerts", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/alerts/SBER.json";

                List<MegaAlertsAssetsDTO> response = await moexHttpAlgClient.GetMegaAlerts(
                    url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2024-04-28",
                        ["till"] = "2026-04-30"
                    },
                    ct);

                return Results.Json(response, AppJsonContext.Default.ListMegaAlertsAssetsDTO);
            });

            routes.MapGet("/GetMegaAlertsFutures", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/alerts/SiM6.json";

                List<MegaAlertsFuturesDTO> response = await moexHttpAlgClient.GetMegaAlertsFutures(
                    url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2026-01-28",
                        ["till"] = "2026-04-30"
                    },
                    ct);

                return Results.Json(response, AppJsonContext.Default.ListMegaAlertsFuturesDTO);
            });


            routes.MapGet("/GetSuperCandlesTradeStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/tradestats/SMLT.json";
                List<SuperCandlesTradeStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesTradeStats5m(url,
                    new Dictionary<string, string>
                    {
                        [ "from"]= "2024-04-08" ,
                        [ "till"]= "2026-04-17"
                        
                    },
                    ct
                    );
                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesTradeStats5mDTO);
            });
            routes.MapGet("/GetSuperCandlesOrderStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/orderstats/SMLT.json";
                List<SuperCandlesOrderStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesOrderStats5m(url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2024-04-08",
                        ["till"] = "2026-04-17"
                    },
                    ct
                    );
                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderStats5mDTO);
            });
            routes.MapGet("/GetSuperCandlesOrderBookStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/obstats/SMLT.json";
                List<SuperCandlesOrderBookStats5mDTO> response = await moexHttpAlgClient.GetSuperCandlesOrderBookStats5m(url,
                    new Dictionary<string, string>
                    {
                        ["from"] = "2024-04-08",
                        ["till"] = "2026-04-17"
                    },
                    ct
                    );
                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderBookStats5mDTO);
            });
            routes.MapGet("/GetCandlesAsset", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) => {
                string url = "/engines/stock/markets/shares/boards/tqbr/securities/SMLT/candles.json";
                List<CandlesDTO> response = await moexHttpAlgClient.GetCandles(url,
                    new Dictionary<string, string>
                    {
                        [ "interval"]= "1" ,
                        [ "from"]= "2024-04-08" ,
                        [ "till"]= "2026-04-17"
                        
                    },
                    ct
                    );
                return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
            });

            routes.MapGet("/GetCandlesFutures", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) => {
                string url = "/engines/futures/markets/forts/boards/RFUD/securities/SiM6/candles.json";
                List<CandlesDTO> response = await moexHttpAlgClient.GetCandles(url,
                    new Dictionary<string, string>
                    {
                        ["interval"] = "1",
                        ["from"] = "2026-01-28",
                        ["till"] = "2026-05-05"
                    },
                    ct);
                return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
            });

            return routes;
        }
    }
}
