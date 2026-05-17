using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class OrderBookStatsMapper
{
    private const string Category = MappingCategories.OrderBookStats;
    private const int IntervalSeconds = 300;

    public static OrderBookStats5m Map(
        SuperCandlesOrderBookStats5mDTO dto,
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
            // secId|beginUtcTicks|300|source|spreadBbo|spreadLv10|spread1Mio|
            // levelsB|levelsS|volB|volS|valB|valS|
            // imbalanceVolBbo|imbalanceValBbo|imbalanceVol|imbalanceVal|
            // vwapB|vwapS|vwapB1Mio|vwapS1Mio
            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{IntervalSeconds}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.SpreadBbo)}|{RowHashHelper.Fmt(dto.SpreadLv10)}|{RowHashHelper.Fmt(dto.Spread1Mio)}|{RowHashHelper.Fmt(dto.LevelsB)}|{RowHashHelper.Fmt(dto.LevelsS)}|{RowHashHelper.Fmt(dto.VolB)}|{RowHashHelper.Fmt(dto.VolS)}|{RowHashHelper.Fmt(dto.ValB)}|{RowHashHelper.Fmt(dto.ValS)}|{RowHashHelper.Fmt(dto.ImbalanceVolBbo)}|{RowHashHelper.Fmt(dto.ImbalanceValBbo)}|{RowHashHelper.Fmt(dto.ImbalanceVol)}|{RowHashHelper.Fmt(dto.ImbalanceVal)}|{RowHashHelper.Fmt(dto.VwapB)}|{RowHashHelper.Fmt(dto.VwapS)}|{RowHashHelper.Fmt(dto.VwapB1Mio)}|{RowHashHelper.Fmt(dto.VwapS1Mio)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new OrderBookStats5m
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                IntervalSeconds = IntervalSeconds,
                Source = ctx.SourceCode,

                SpreadBbo = dto.SpreadBbo,
                SpreadLv10 = dto.SpreadLv10,
                Spread1Mio = dto.Spread1Mio,

                LevelsB = dto.LevelsB,
                LevelsS = dto.LevelsS,

                VolB = dto.VolB,
                VolS = dto.VolS,
                ValB = dto.ValB,
                ValS = dto.ValS,

                ImbalanceVolBbo = dto.ImbalanceVolBbo,
                ImbalanceValBbo = dto.ImbalanceValBbo,
                ImbalanceVol = dto.ImbalanceVol,
                ImbalanceVal = dto.ImbalanceVal,

                VwapB = dto.VwapB,
                VwapS = dto.VwapS,
                VwapB1Mio = dto.VwapB1Mio,
                VwapS1Mio = dto.VwapS1Mio,

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

    public static List<OrderBookStats5m> MapBatch(
        IReadOnlyList<SuperCandlesOrderBookStats5mDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<OrderBookStats5m>(dtos.Count);
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
