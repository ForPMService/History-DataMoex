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
            routes.MapGet("/calendar/offdays-all", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetOffDaysAll(), AppJsonContext.Default.ListCalendarOffDaysAllDTO));

            routes.MapGet("/calendar/stock-offdays", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetStockOffDays(), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

            routes.MapGet("/calendar/futures-offdays", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetFuturesOffDays(), AppJsonContext.Default.ListCalendarOffDaysMarketDTO));

            routes.MapGet("/calendar/stock-session", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetStockSession(), AppJsonContext.Default.ListCalendarStockSessionDTO));

            routes.MapGet("/calendar/stock-session-types", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetStockSessionTypes(), AppJsonContext.Default.ListCalendarSessionTypeDTO));

            routes.MapGet("/calendar/futures-session", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetFuturesSession(), AppJsonContext.Default.ListCalendarFuturesSessionDTO));

            routes.MapGet("/calendar/futures-session-types", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetFuturesSessionTypes(), AppJsonContext.Default.ListCalendarSessionTypeDTO));

            routes.MapGet("/calendar/forts-contracts", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetFortsContracts(), AppJsonContext.Default.ListCalendarFortsContractDTO));

            routes.MapGet("/calendar/options-series", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetOptionsSeries(), AppJsonContext.Default.ListCalendarOptionsSeriesDTO));

            routes.MapGet("/calendar/suspended-reasons", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetSuspendedReasons(), AppJsonContext.Default.ListCalendarSuspendedReasonDTO));

            routes.MapGet("/calendar/suspended", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetSuspended(), AppJsonContext.Default.ListCalendarSuspendedDTO));

            routes.MapGet("/calendar/security-attributes", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetSecurityAttributes(), AppJsonContext.Default.ListCalendarSecurityAttributeDTO));

            routes.MapGet("/calendar/security-changes", async (MoexHttpCalendarClient c) =>
                Results.Json(await c.GetSecurityChanges(), AppJsonContext.Default.ListCalendarSecurityChangeDTO));

            return routes;
        }
    }
}
