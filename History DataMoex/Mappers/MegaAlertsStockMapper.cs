using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static class MegaAlertsStockMapper
{
    private const string Category = MappingCategories.MegaAlertsStock;

    public static MegaAlertsStock Map(
        MegaAlertsAssetsDTO dto,
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

            // Каноническая строка: secId|beginUtcTicks|source|alertType|threshold|value|reference
            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{secId}|{beginUtc.Ticks}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.AlertType)}|{RowHashHelper.Fmt(dto.Threshold)}|{RowHashHelper.Fmt(dto.Value)}|{RowHashHelper.Fmt(dto.Reference)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new MegaAlertsStock
            {
                SecId = secId,
                BeginUtc = beginUtc,
                BeginLocal = beginLocal,
                Source = ctx.SourceCode,

                AlertType = dto.AlertType,
                Threshold = dto.Threshold,
                Value = dto.Value,
                Reference = dto.Reference,

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

    public static List<MegaAlertsStock> MapBatch(
        IReadOnlyList<MegaAlertsAssetsDTO> dtos,
        string secId,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId, Category, dtos.Count);

        var result = new List<MegaAlertsStock>(dtos.Count);
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
