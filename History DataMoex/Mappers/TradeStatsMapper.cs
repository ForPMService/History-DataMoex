using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class TradeStatsMapper
{
    private const string Category = MappingCategories.TradeStats;
    private const int IntervalSeconds = 300;

    public static TradeStats5m Map(
        SuperCandlesTradeStats5mDTO dto,
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

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{IntervalSeconds}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.PrOpen)}|{RowHashHelper.Fmt(dto.PrHigh)}|{RowHashHelper.Fmt(dto.PrLow)}|{RowHashHelper.Fmt(dto.PrClose)}|{RowHashHelper.Fmt(dto.PrStd)}|{RowHashHelper.Fmt(dto.Vol)}|{RowHashHelper.Fmt(dto.Val)}|{RowHashHelper.Fmt(dto.Trades)}|{RowHashHelper.Fmt(dto.PrVwap)}|{RowHashHelper.Fmt(dto.PrChange)}|{RowHashHelper.Fmt(dto.TradesB)}|{RowHashHelper.Fmt(dto.TradesS)}|{RowHashHelper.Fmt(dto.ValB)}|{RowHashHelper.Fmt(dto.ValS)}|{RowHashHelper.Fmt(dto.VolB)}|{RowHashHelper.Fmt(dto.VolS)}|{RowHashHelper.Fmt(dto.Disb)}|{RowHashHelper.Fmt(dto.PrVwapB)}|{RowHashHelper.Fmt(dto.PrVwapS)}|{RowHashHelper.Fmt(dto.SecPrOpen)}|{RowHashHelper.Fmt(dto.SecPrHigh)}|{RowHashHelper.Fmt(dto.SecPrLow)}|{RowHashHelper.Fmt(dto.SecPrClose)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new TradeStats5m
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                IntervalSeconds = IntervalSeconds,
                Source = ctx.SourceCode,

                PrOpen = dto.PrOpen,
                PrHigh = dto.PrHigh,
                PrLow = dto.PrLow,
                PrClose = dto.PrClose,
                PrStd = dto.PrStd,

                Vol = dto.Vol,
                Val = dto.Val,
                Trades = dto.Trades,

                PrVwap = dto.PrVwap,
                PrChange = dto.PrChange,

                TradesB = dto.TradesB,
                TradesS = dto.TradesS,
                ValB = dto.ValB,
                ValS = dto.ValS,
                VolB = dto.VolB,
                VolS = dto.VolS,

                Disb = dto.Disb,
                PrVwapB = dto.PrVwapB,
                PrVwapS = dto.PrVwapS,

                SecPrOpen = dto.SecPrOpen,
                SecPrHigh = dto.SecPrHigh,
                SecPrLow = dto.SecPrLow,
                SecPrClose = dto.SecPrClose,

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

    public static List<TradeStats5m> MapBatch(
        IReadOnlyList<SuperCandlesTradeStats5mDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<TradeStats5m>(dtos.Count);
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
