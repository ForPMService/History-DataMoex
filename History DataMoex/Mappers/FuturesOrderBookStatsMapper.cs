using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class FuturesOrderBookStatsMapper
{
    private const string Category = MappingCategories.FuturesOrderBookStats;
    private const int IntervalSeconds = 300;

    public static FuturesOrderBookStats5m Map(
        SuperCandlesFuturesOrderBookStats5mDTO dto,
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
            // secId|beginUtcTicks|300|source|assetCode|midPrice|microPrice|
            // spreadL1..L20 (6 полей)|levelsB|levelsS|
            // volBL1..L20 (6 полей)|volSL1..L20 (6 полей)|
            // vwapBL3..L20 (4 поля)|vwapSL3..L20 (4 поля)
            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{IntervalSeconds}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.AssetCode)}|{RowHashHelper.Fmt(dto.MidPrice)}|{RowHashHelper.Fmt(dto.MicroPrice)}|{RowHashHelper.Fmt(dto.SpreadL1)}|{RowHashHelper.Fmt(dto.SpreadL2)}|{RowHashHelper.Fmt(dto.SpreadL3)}|{RowHashHelper.Fmt(dto.SpreadL5)}|{RowHashHelper.Fmt(dto.SpreadL10)}|{RowHashHelper.Fmt(dto.SpreadL20)}|{RowHashHelper.Fmt(dto.LevelsB)}|{RowHashHelper.Fmt(dto.LevelsS)}|{RowHashHelper.Fmt(dto.VolBL1)}|{RowHashHelper.Fmt(dto.VolBL2)}|{RowHashHelper.Fmt(dto.VolBL3)}|{RowHashHelper.Fmt(dto.VolBL5)}|{RowHashHelper.Fmt(dto.VolBL10)}|{RowHashHelper.Fmt(dto.VolBL20)}|{RowHashHelper.Fmt(dto.VolSL1)}|{RowHashHelper.Fmt(dto.VolSL2)}|{RowHashHelper.Fmt(dto.VolSL3)}|{RowHashHelper.Fmt(dto.VolSL5)}|{RowHashHelper.Fmt(dto.VolSL10)}|{RowHashHelper.Fmt(dto.VolSL20)}|{RowHashHelper.Fmt(dto.VwapBL3)}|{RowHashHelper.Fmt(dto.VwapBL5)}|{RowHashHelper.Fmt(dto.VwapBL10)}|{RowHashHelper.Fmt(dto.VwapBL20)}|{RowHashHelper.Fmt(dto.VwapSL3)}|{RowHashHelper.Fmt(dto.VwapSL5)}|{RowHashHelper.Fmt(dto.VwapSL10)}|{RowHashHelper.Fmt(dto.VwapSL20)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new FuturesOrderBookStats5m
            {
                SecId = secId,
                AssetCode = dto.AssetCode,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                IntervalSeconds = IntervalSeconds,
                Source = ctx.SourceCode,

                MidPrice = dto.MidPrice,
                MicroPrice = dto.MicroPrice,

                SpreadL1 = dto.SpreadL1,
                SpreadL2 = dto.SpreadL2,
                SpreadL3 = dto.SpreadL3,
                SpreadL5 = dto.SpreadL5,
                SpreadL10 = dto.SpreadL10,
                SpreadL20 = dto.SpreadL20,

                LevelsB = dto.LevelsB,
                LevelsS = dto.LevelsS,

                VolBL1 = dto.VolBL1,
                VolBL2 = dto.VolBL2,
                VolBL3 = dto.VolBL3,
                VolBL5 = dto.VolBL5,
                VolBL10 = dto.VolBL10,
                VolBL20 = dto.VolBL20,

                VolSL1 = dto.VolSL1,
                VolSL2 = dto.VolSL2,
                VolSL3 = dto.VolSL3,
                VolSL5 = dto.VolSL5,
                VolSL10 = dto.VolSL10,
                VolSL20 = dto.VolSL20,

                VwapBL3 = dto.VwapBL3,
                VwapBL5 = dto.VwapBL5,
                VwapBL10 = dto.VwapBL10,
                VwapBL20 = dto.VwapBL20,

                VwapSL3 = dto.VwapSL3,
                VwapSL5 = dto.VwapSL5,
                VwapSL10 = dto.VwapSL10,
                VwapSL20 = dto.VwapSL20,

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

    public static List<FuturesOrderBookStats5m> MapBatch(
        IReadOnlyList<SuperCandlesFuturesOrderBookStats5mDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<FuturesOrderBookStats5m>(dtos.Count);
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
