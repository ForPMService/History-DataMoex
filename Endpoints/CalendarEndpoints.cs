using History_DataMoex.Clients;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Serialization;

namespace History_DataMoex.Endpoints
{
    /// <summary>
    /// Source endpoint-ы MOEX: в момент запроса идут в MOEX,
    /// парсят ответ и возвращают DTO MOEX.
    /// Это не ручки витрины для фронта; ручки витрины появятся позже
    /// и будут читать данные из PostgreSQL/ClickHouse.
    /// </summary>
    public static class CalendarEndpoints
    {
        public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder routes)
        {
            // === ISS Календарь — Обзор ===
            routes.MapGet("/calendar/offdays-all", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetOffDaysAll(ct), AppJsonContext.Default.ListCalendarOffDaysAllDTO));

            routes.MapGet("/calendar/stock-offdays", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetStockOffDays(ct), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

            routes.MapGet("/calendar/futures-offdays", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetFuturesOffDays(ct), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

            routes.MapGet("/calendar/stock-session", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetStockSession(ct), AppJsonContext.Default.ListCalendarStockSessionDTO));

            routes.MapGet("/calendar/stock-session-types", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetStockSessionTypes(ct), AppJsonContext.Default.ListCalendarSessionTypeDTO));

            routes.MapGet("/calendar/futures-session", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetFuturesSession(ct), AppJsonContext.Default.ListCalendarFuturesSessionDTO));

            routes.MapGet("/calendar/futures-session-types", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetFuturesSessionTypes(ct), AppJsonContext.Default.ListCalendarSessionTypeDTO));

            routes.MapGet("/calendar/forts-contracts", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetFortsContracts(ct), AppJsonContext.Default.ListCalendarFortsContractDTO));

            routes.MapGet("/calendar/options-series", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetOptionsSeries(ct), AppJsonContext.Default.ListCalendarOptionsSeriesDTO));

            routes.MapGet("/calendar/suspended-reasons", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetSuspendedReasons(ct), AppJsonContext.Default.ListCalendarSuspendedReasonDTO));

            routes.MapGet("/calendar/suspended", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
            {
                List<CalendarSuspendedDTO> response = new List<CalendarSuspendedDTO>();
                await foreach (List<CalendarSuspendedDTO> page in c.GetSuspended(ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListCalendarSuspendedDTO);
            });

            routes.MapGet("/calendar/security-attributes", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
                Results.Json(await c.GetSecurityAttributes(ct), AppJsonContext.Default.ListCalendarSecurityAttributeDTO));

            routes.MapGet("/calendar/security-changes", async (
                MoexHttpCalendarClient c,
                CancellationToken ct) =>
            {
                List<CalendarSecurityChangeDTO> response = new List<CalendarSecurityChangeDTO>();
                await foreach (List<CalendarSecurityChangeDTO> page in c.GetSecurityChanges(ct))
                {
                    response.AddRange(page);
                }

                return Results.Json(response, AppJsonContext.Default.ListCalendarSecurityChangeDTO);
            });

            return routes;
        }
    }
}
