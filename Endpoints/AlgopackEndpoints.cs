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

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-01-28",
                    ["till"] = "2026-05-05"
                };

                List<SuperCandlesFuturesTradeStats5mDTO> response =
                    new List<SuperCandlesFuturesTradeStats5mDTO>();

                await foreach (List<SuperCandlesFuturesTradeStats5mDTO> page in moexHttpAlgClient.GetSuperCandlesFuturesTradeStats5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesTradeStats5mDTO);
            });

            routes.MapGet("/GetSuperCandlesFuturesOrderBookStat", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/obstats/SiM6.json";

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-01-28",
                    ["till"] = "2026-04-30"
                };

                List<SuperCandlesFuturesOrderBookStats5mDTO> response =
                    new List<SuperCandlesFuturesOrderBookStats5mDTO>();

                await foreach (List<SuperCandlesFuturesOrderBookStats5mDTO> page in moexHttpAlgClient.GetSuperCandlesFuturesOrderBookStats5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesFuturesOrderBookStats5mDTO);
            });


            // === FUTOI ===
            routes.MapGet("/GetFutoi", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/analyticalproducts/futoi/securities/Si.json";

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-05-03",
                    ["till"] = "2026-05-08"
                };

                List<FutoiDTO> response = new List<FutoiDTO>();
                await foreach (List<FutoiDTO> page in moexHttpAlgClient.StreamFutoi(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListFutoiDTO);
            });

            // === HI2 ===
            routes.MapGet("/GetHi2Asset", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/hi2/SBER.json";

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-05-03",
                    ["till"] = "2026-05-03"
                };

                List<Hi2AssetDTO> response = new List<Hi2AssetDTO>();
                await foreach (List<Hi2AssetDTO> page in moexHttpAlgClient.GetHi2Asset5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListHi2AssetDTO);
            });

            routes.MapGet("/GetHi2Furure", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/hi2/SiM6.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-01-30",
                    ["till"] = "2026-05-04"
                };

                List<Hi2FuturesDTO> response = new List<Hi2FuturesDTO>();
                await foreach (List<Hi2FuturesDTO> page in moexHttpAlgClient.GetHi2Furures5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListHi2FuturesDTO);
            });

             // === Мега-оповещения ===
            routes.MapGet("/GetMegaAlerts", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/alerts/SBER.json";

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2024-04-28",
                    ["till"] = "2026-04-30"
                };

                List<MegaAlertsAssetsDTO> response = new List<MegaAlertsAssetsDTO>();
                await foreach (List<MegaAlertsAssetsDTO> page in moexHttpAlgClient.GetMegaAlerts(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListMegaAlertsAssetsDTO);
            });

            routes.MapGet("/GetMegaAlertsFutures", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/fo/alerts/SiM6.json";

                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2026-01-28",
                    ["till"] = "2026-04-30"
                };

                List<MegaAlertsFuturesDTO> response = new List<MegaAlertsFuturesDTO>();
                await foreach (List<MegaAlertsFuturesDTO> page in moexHttpAlgClient.GetMegaAlertsFutures(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListMegaAlertsFuturesDTO);
            });


            routes.MapGet("/GetSuperCandlesTradeStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/tradestats/SMLT.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2024-04-08",
                    ["till"] = "2026-04-17"
                };

                List<SuperCandlesTradeStats5mDTO> response = new List<SuperCandlesTradeStats5mDTO>();
                await foreach (List<SuperCandlesTradeStats5mDTO> page in moexHttpAlgClient.GetSuperCandlesTradeStats5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesTradeStats5mDTO);
            });
            routes.MapGet("/GetSuperCandlesOrderStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/orderstats/SMLT.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2024-04-08",
                    ["till"] = "2026-04-17"
                };

                List<SuperCandlesOrderStats5mDTO> response = new List<SuperCandlesOrderStats5mDTO>();
                await foreach (List<SuperCandlesOrderStats5mDTO> page in moexHttpAlgClient.GetSuperCandlesOrderStats5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderStats5mDTO);
            });
            routes.MapGet("/GetSuperCandlesOrderBookStats", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/datashop/algopack/eq/obstats/SMLT.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["from"] = "2024-04-08",
                    ["till"] = "2026-04-17"
                };

                List<SuperCandlesOrderBookStats5mDTO> response = new List<SuperCandlesOrderBookStats5mDTO>();
                await foreach (List<SuperCandlesOrderBookStats5mDTO> page in moexHttpAlgClient.GetSuperCandlesOrderBookStats5m(url, queryParams, ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListSuperCandlesOrderBookStats5mDTO);
            });
            routes.MapGet("/GetCandlesAsset", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/engines/stock/markets/shares/boards/tqbr/securities/SMLT/candles.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["interval"] = "1",
                    ["from"] = "2026-04-17",
                    ["till"] = "2026-04-20"

                };
                List<CandlesDTO> response = new List<CandlesDTO>();
                await foreach (List<CandlesDTO> candlesBatch in moexHttpAlgClient.GetCandles(url, queryParams, ct))
                {
                    response.AddRange(candlesBatch);
                }
                return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
            });



            routes.MapGet("/GetCandlesFutures", async (
                MoexHttpAlgClient moexHttpAlgClient,
                CancellationToken ct) =>
            {
                string url = "/engines/futures/markets/forts/boards/RFUD/securities/SiM6/candles.json";
                Dictionary<string, string> queryParams = new Dictionary<string, string>
                {
                    ["interval"] = "1",
                    ["from"] = "2026-01-28",
                    ["till"] = "2026-05-05"
                };
                    
                List<CandlesDTO> response = new List<CandlesDTO>();
                await foreach (List<CandlesDTO> candlesBatch in moexHttpAlgClient.GetCandles(url, queryParams, ct))
                {
                    response.AddRange(candlesBatch);
                }

                return Results.Json(response, AppJsonContext.Default.ListCandlesDTO);
            });

            return routes;
        }
    }
}
