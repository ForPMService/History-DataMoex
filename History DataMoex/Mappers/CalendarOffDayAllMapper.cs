using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarOffDaysAllDTO → CalendarOffDayAll.
/// Group C — SecId not applicable (Lock §9). TradeDate required.
/// long? Workday → int? (значения 0/1).
/// </summary>
public static class CalendarOffDayAllMapper
{
    private const string Category = MappingCategories.CalendarOffDayAll;

    public static CalendarOffDayAll Map(
        CalendarOffDaysAllDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            _ = sourceTz;

            DateOnly tradeDate = SourceDateTimeParser.ParseRequiredDateOnly(dto.TradeDate, "trade_date");
            DateOnly? currencyTradeSessionDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.CurrencyTradeSessionDate, "currency_trade_session_date");
            DateOnly? futuresTradeSessionDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.FuturesTradeSessionDate, "futures_trade_session_date");
            DateOnly? stockTradeSessionDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.StockTradeSessionDate, "stock_trade_session_date");

            int? currencyWorkday = (int?)dto.CurrencyWorkday;
            int? futuresWorkday = (int?)dto.FuturesWorkday;
            int? stockWorkday = (int?)dto.StockWorkday;

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{RowHashHelper.Fmt((DateOnly?)tradeDate)}|{ctx.SourceCode}|{RowHashHelper.Fmt(currencyWorkday)}|{RowHashHelper.Fmt(currencyTradeSessionDate)}|{RowHashHelper.Fmt(dto.CurrencyReason)}|{RowHashHelper.Fmt(futuresWorkday)}|{RowHashHelper.Fmt(futuresTradeSessionDate)}|{RowHashHelper.Fmt(dto.FuturesReason)}|{RowHashHelper.Fmt(stockWorkday)}|{RowHashHelper.Fmt(stockTradeSessionDate)}|{RowHashHelper.Fmt(dto.StockReason)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarOffDayAll
            {
                TradeDate = tradeDate,
                CurrencyWorkday = currencyWorkday,
                CurrencyTradeSessionDate = currencyTradeSessionDate,
                CurrencyReason = dto.CurrencyReason,
                FuturesWorkday = futuresWorkday,
                FuturesTradeSessionDate = futuresTradeSessionDate,
                FuturesReason = dto.FuturesReason,
                StockWorkday = stockWorkday,
                StockTradeSessionDate = stockTradeSessionDate,
                StockReason = dto.StockReason,

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

    public static List<CalendarOffDayAll> MapBatch(
        IReadOnlyList<CalendarOffDaysAllDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarOffDayAll>(dtos.Count);
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
