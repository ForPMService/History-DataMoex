using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using History_DataMoex.Clients;
using History_DataMoex.DataTransfers;
using History_DataMoex.Parsing;

namespace History_DataMoex.Endpoints
{
    /// <summary>
    /// Эндпоинты справочников инструментов MOEX ISS.
    /// Эти эндпоинты напрямую возвращают текущие DTO-ответы MOEX.
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
