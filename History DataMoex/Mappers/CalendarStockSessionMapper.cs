using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarStockSessionDTO → CalendarStockSession.
/// Group B — SecId nullable (Lock §9). TradeDate required. TimeFrom/TimeTill — TimeOnly?, не UTC (Lock §4).
/// UpdateTimeUtc — MSK→UTC (Lock §4).
/// </summary>
public static class CalendarStockSessionMapper
{
    private const string Category = MappingCategories.CalendarStockSession;

    public static CalendarStockSession Map(
        CalendarStockSessionDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        string? normalizedSecId = (string.IsNullOrEmpty(dto.SecId) || dto.SecId == "-")
            ? null
            : dto.SecId;

        try
        {
            DateOnly tradeDate = SourceDateTimeParser.ParseRequiredDateOnly(dto.TradeDate, "trade_date");
            TimeOnly? timeFrom = SourceDateTimeParser.ParseTimeOnlyOrNull(dto.TimeFrom, "time_from");
            TimeOnly? timeTill = SourceDateTimeParser.ParseTimeOnlyOrNull(dto.TimeTill, "time_till");
            DateTime? updateTimeUtc = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(dto.UpdateTime, sourceTz, "update_time");

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{RowHashHelper.Fmt((DateOnly?)tradeDate)}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.TradingSession)}|{RowHashHelper.Fmt(dto.BoardId)}|{RowHashHelper.Fmt(normalizedSecId)}|{RowHashHelper.Fmt(dto.Type)}|{RowHashHelper.Fmt(timeFrom)}|{RowHashHelper.Fmt(timeTill)}|{RowHashHelper.FmtUtc(updateTimeUtc)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarStockSession
            {
                TradeDate = tradeDate,
                TradingSession = dto.TradingSession,
                BoardId = dto.BoardId,
                SecId = normalizedSecId,
                Type = dto.Type,
                TimeFrom = timeFrom,
                TimeTill = timeTill,
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
            if (normalizedSecId != null)
                ex.WithContext(Category, normalizedSecId, rowIndex);
            else
                ex.WithContext(Category, rowIndex);
            throw;
        }
    }

    public static List<CalendarStockSession> MapBatch(
        IReadOnlyList<CalendarStockSessionDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarStockSession>(dtos.Count);
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
                    result.Add(Map(dtos[i], ctx, sourceTz, i));
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
