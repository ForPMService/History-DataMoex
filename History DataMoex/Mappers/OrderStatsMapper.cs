using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class OrderStatsMapper
{
    private const string Category = MappingCategories.OrderStats;
    private const int IntervalSeconds = 300;

    public static OrderStats5m Map(
        SuperCandlesOrderStats5mDTO dto,
        string secId,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            if (!string.IsNullOrEmpty(dto.SecId)
                && !string.Equals(dto.SecId, secId, StringComparison.Ordinal))
            {
                throw new MappingValidationException(
                    $"DTO SecId '{dto.SecId}' does not match expected '{secId}' at row {rowIndex}",
                    category: Category, secId: secId, rowIndex: rowIndex);
            }

            (DateTime beginUtc, DateTime beginLocal) =
                TradeDateTimeParser.ParseToUtc(dto.TradeDate, dto.TradeTime, sourceTz);

            // Каноническая строка:
            // secId|beginUtcTicks|300|source|
            // putOrdersB|putOrdersS|putValB|putValS|putVolB|putVolS|putVwapB|putVwapS|putVol|putVal|putOrders|
            // cancelOrdersB|cancelOrdersS|cancelValB|cancelValS|cancelVolB|cancelVolS|cancelVwapB|cancelVwapS|cancelVol|cancelVal|cancelOrders
            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{IntervalSeconds}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.PutOrdersB)}|{RowHashHelper.Fmt(dto.PutOrdersS)}|{RowHashHelper.Fmt(dto.PutValB)}|{RowHashHelper.Fmt(dto.PutValS)}|{RowHashHelper.Fmt(dto.PutVolB)}|{RowHashHelper.Fmt(dto.PutVolS)}|{RowHashHelper.Fmt(dto.PutVwapB)}|{RowHashHelper.Fmt(dto.PutVwapS)}|{RowHashHelper.Fmt(dto.PutVol)}|{RowHashHelper.Fmt(dto.PutVal)}|{RowHashHelper.Fmt(dto.PutOrders)}|{RowHashHelper.Fmt(dto.CancelOrdersB)}|{RowHashHelper.Fmt(dto.CancelOrdersS)}|{RowHashHelper.Fmt(dto.CancelValB)}|{RowHashHelper.Fmt(dto.CancelValS)}|{RowHashHelper.Fmt(dto.CancelVolB)}|{RowHashHelper.Fmt(dto.CancelVolS)}|{RowHashHelper.Fmt(dto.CancelVwapB)}|{RowHashHelper.Fmt(dto.CancelVwapS)}|{RowHashHelper.Fmt(dto.CancelVol)}|{RowHashHelper.Fmt(dto.CancelVal)}|{RowHashHelper.Fmt(dto.CancelOrders)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new OrderStats5m
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                IntervalSeconds = IntervalSeconds,
                Source = ctx.SourceCode,

                PutOrdersB = dto.PutOrdersB,
                PutOrdersS = dto.PutOrdersS,
                PutValB = dto.PutValB,
                PutValS = dto.PutValS,
                PutVolB = dto.PutVolB,
                PutVolS = dto.PutVolS,
                PutVwapB = dto.PutVwapB,
                PutVwapS = dto.PutVwapS,
                PutVol = dto.PutVol,
                PutVal = dto.PutVal,
                PutOrders = dto.PutOrders,

                CancelOrdersB = dto.CancelOrdersB,
                CancelOrdersS = dto.CancelOrdersS,
                CancelValB = dto.CancelValB,
                CancelValS = dto.CancelValS,
                CancelVolB = dto.CancelVolB,
                CancelVolS = dto.CancelVolS,
                CancelVwapB = dto.CancelVwapB,
                CancelVwapS = dto.CancelVwapS,
                CancelVol = dto.CancelVol,
                CancelVal = dto.CancelVal,
                CancelOrders = dto.CancelOrders,

                RowHashV1 = rowHash,
                RawObjectId = ctx.RawObjectId,
                LoadJobId = ctx.LoadJobId,
            };
        }
        catch (MappingException ex)
        {
            ex.WithContext(Category, secId, rowIndex);
            throw;
        }
    }

    public static List<OrderStats5m> MapBatch(
        IReadOnlyList<SuperCandlesOrderStats5mDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<OrderStats5m>(dtos.Count);
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
                    result.Add(Map(dtos[i], secId, ctx, sourceTz, i));
                }
                catch (MappingException rowEx)
                {
                    rowErrorLogged = true;
                    MappingLogMessages.MapRowFailed(
                        logger, rowEx, ctx.SourceCode, secId, Category, i,
                        rowEx.ErrorCategory, rowEx.Message);
                    throw;
                }
            }
        }
        catch (OperationCanceledException ocEx)
        {
            MappingLogMessages.MapBatchCancelled(
                logger, ocEx, ctx.SourceCode, secId, Category, result.Count);
            throw;
        }
        catch (MappingException batchEx) when (!rowErrorLogged)
        {
            MappingLogMessages.MapBatchFailed(
                logger, batchEx, ctx.SourceCode, secId, Category,
                batchEx.ErrorCategory, batchEx.Message);
            throw;
        }

        MappingLogMessages.MapBatchCompleted(
            logger, ctx.SourceCode, secId, Category, result.Count,
            Stopwatch.GetElapsedTime(started));
        return result;
    }
}
