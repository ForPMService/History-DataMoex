using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarOffDaysMarketDTO → CalendarMarketOffDay.
/// Group C + market (Lock §9). Market — внешний параметр (whitelist "stock"/"futures").
/// TradeDate required. UpdateTimeUtc — MSK→UTC (Lock §4).
/// </summary>
public static class CalendarMarketOffDayMapper
{
    private const string Category = MappingCategories.CalendarMarketOffDay;

    public static CalendarMarketOffDay Map(
        CalendarOffDaysMarketDTO dto,
        string market,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            if (market is not ("stock" or "futures"))
            {
                throw new MappingValidationException(
                    $"{Category} requires market parameter 'stock' or 'futures' at row {rowIndex}, got '{market ?? "(null)"}'",
                    category: Category,
                    rowIndex: rowIndex);
            }

            DateOnly tradeDate = SourceDateTimeParser.ParseRequiredDateOnly(dto.TradeDate, "trade_date");
            DateOnly? tradeSessionDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.TradeSessionDate, "trade_session_date");
            DateTime? updateTimeUtc = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(dto.UpdateTime, sourceTz, "update_time");

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{market}|{RowHashHelper.Fmt((DateOnly?)tradeDate)}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.IsTraded)}|{RowHashHelper.Fmt(tradeSessionDate)}|{RowHashHelper.Fmt(dto.Reason)}|{RowHashHelper.FmtUtc(updateTimeUtc)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarMarketOffDay
            {
                Market = market,
                TradeDate = tradeDate,
                IsTraded = dto.IsTraded,
                TradeSessionDate = tradeSessionDate,
                Reason = dto.Reason,
                UpdateTimeUtc = updateTimeUtc,

                Source = ctx.SourceCode,
                RowHashV1 = rowHash,
                RawObjectId = ctx.RawObjectId,
                LoadJobId = ctx.LoadJobId,
                FetchedAtUtc = ctx.FetchedAtUtc,
            };
        }
        catch (MappingException ex)
        {
            ex.WithContext(Category, rowIndex);
            throw;
        }
    }

    public static List<CalendarMarketOffDay> MapBatch(
        IReadOnlyList<CalendarOffDaysMarketDTO> dtos,
        string market,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarMarketOffDay>(dtos.Count);
        bool rowErrorLogged = false;

        try
        {
            TimeZoneInfo sourceTz = SourceTimezones.Resolve(ctx.SourceTimezone);

            for (int i = 0; i < dtos.Count; i++)
            {
                if ((i & 0xFFF) == 0)
                    ct.ThrowIfCancellationRequested();

                try
                {
                    result.Add(Map(dtos[i], market, ctx, sourceTz, i));
                }
                catch (MappingException rowEx)
                {
                    rowErrorLogged = true;
                    MappingLogMessages.MapRowFailed(
                        logger, rowEx, ctx.SourceCode, rowEx.SecId ?? string.Empty, Category, i,
                        rowEx.ErrorCategory, rowEx.Message);
                    throw;
                }
            }
        }
        catch (OperationCanceledException ocEx)
        {
            MappingLogMessages.MapBatchCancelled(
                logger, ocEx, ctx.SourceCode, secId: string.Empty, Category, result.Count);
            throw;
        }
        catch (MappingException batchEx) when (!rowErrorLogged)
        {
            MappingLogMessages.MapBatchFailed(
                logger, batchEx, ctx.SourceCode, secId: string.Empty, Category,
                batchEx.ErrorCategory, batchEx.Message);
            throw;
        }

        MappingLogMessages.MapBatchCompleted(
            logger, ctx.SourceCode, secId: string.Empty, Category, result.Count,
            Stopwatch.GetElapsedTime(started));

        return result;
    }
}
