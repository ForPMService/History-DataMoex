using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class CandlesMapper
{
    private const string Category = MappingCategories.Candles;
    private const int IntervalSeconds = 60;

    public static Candle1m Map(
        CandlesDTO dto,
        string secId,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            if (dto.Begin is null)
                throw new MappingValidationException(
                    "Candle Begin is null",
                    category: Category,
                    secId: secId,
                    rowIndex: rowIndex);

            DateTime beginLocal = DateTime.SpecifyKind(dto.Begin.Value, DateTimeKind.Unspecified);
            DateTime beginUtc = TimeZoneInfo.ConvertTimeToUtc(beginLocal, sourceTz);

            // Каноническая строка для RowHashV1.
            // Состав: secId|beginUtcTicks|60|source|open|high|low|close|volume|value
            string canonical = string.Create(CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{IntervalSeconds}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.Open)}|{RowHashHelper.Fmt(dto.High)}|{RowHashHelper.Fmt(dto.Low)}|{RowHashHelper.Fmt(dto.Close)}|{RowHashHelper.Fmt(dto.Volume)}|{RowHashHelper.Fmt(dto.Value)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new Candle1m
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                IntervalSeconds = IntervalSeconds,
                Source = ctx.SourceCode,
                Open = dto.Open,
                High = dto.High,
                Low = dto.Low,
                Close = dto.Close,
                Volume = dto.Volume,
                Value = dto.Value,
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

    public static List<Candle1m> MapBatch(
        IReadOnlyList<CandlesDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(
            logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<Candle1m>(dtos.Count);
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
