using History_DataMoex.Clients;
using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Contracts.Serialization;

namespace History_DataMoex.Endpoints
{
    /// <summary>
    /// Source endpoint-ы MOEX: в момент запроса идут в MOEX,
    /// парсят ответ и возвращают DTO MOEX.
    /// Это не ручки витрины для фронта; ручки витрины появятся позже
    /// и будут читать данные из PostgreSQL/ClickHouse.
    /// </summary>
    public static class ReferenceEndpoints
    {
        public static IEndpointRouteBuilder MapReferenceEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapGet("/GetStockMarkets", async (MoexHttpIssClient moexHttpIssClient) => {
                string url = "/engines/stock/markets/shares/boards/tqbr/securities.json";
                List<StockSecurityDTO> response = await moexHttpIssClient.GetInfoTradedStockAssets(url);
                return Results.Json(response, AppJsonContext.Default.ListStockSecurityDTO);
            });

            routes.MapGet("/GetFuturesMarkets", async (MoexHttpIssClient moexHttpIssClient) => {
                string url = "/engines/futures/markets/forts/boards/RFUD/securities.json";
                List<FuturesSecurityDTO> response = await moexHttpIssClient.GetInfoTradedFuturesAssets(url);
                return Results.Json(response, AppJsonContext.Default.ListFuturesSecurityDTO);
            });

            return routes;
        }
    }
}
