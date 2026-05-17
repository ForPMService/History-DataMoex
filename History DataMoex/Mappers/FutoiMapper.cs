using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class FutoiMapper
{
    private const string Category = MappingCategories.Futoi;

    public static Futoi Map(
        FutoiDTO dto,
        string secId,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            if (!string.IsNullOrEmpty(dto.Ticker)
                && !string.Equals(dto.Ticker, secId, StringComparison.Ordinal))
            {
                throw new MappingValidationException(
                    $"FUTOI Ticker '{dto.Ticker}' does not match expected secId '{secId}' at row {rowIndex}",
                    category: Category, secId: secId, rowIndex: rowIndex);
            }

            (DateTime beginUtc, DateTime beginLocal) =
                TradeDateTimeParser.ParseToUtc(dto.TradeDate, dto.TradeTime, sourceTz);

            // Каноническая строка:
            // secId|beginUtcTicks|source|clGroup|sessId|seqNum|
            // pos|posLong|posShort|posLongNum|posShortNum|tradeSessionDate
            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.ClGroup)}|{RowHashHelper.Fmt(dto.SessId)}|{RowHashHelper.Fmt(dto.SeqNum)}|{RowHashHelper.Fmt(dto.Pos)}|{RowHashHelper.Fmt(dto.PosLong)}|{RowHashHelper.Fmt(dto.PosShort)}|{RowHashHelper.Fmt(dto.PosLongNum)}|{RowHashHelper.Fmt(dto.PosShortNum)}|{RowHashHelper.Fmt(dto.TradeSessionDate)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new Futoi
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                Source = ctx.SourceCode,

                SessId = dto.SessId,
                SeqNum = dto.SeqNum,
                ClGroup = dto.ClGroup,

                Pos = dto.Pos,
                PosLong = dto.PosLong,
                PosShort = dto.PosShort,
                PosLongNum = dto.PosLongNum,
                PosShortNum = dto.PosShortNum,
                TradeSessionDate = dto.TradeSessionDate,

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

    public static List<Futoi> MapBatch(
        IReadOnlyList<FutoiDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<Futoi>(dtos.Count);
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
