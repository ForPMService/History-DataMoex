using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarSessionTypeDTO → CalendarSessionType.
/// Group C + market (Lock §9). Market — внешний параметр (whitelist "stock"/"futures").
/// Type — required бизнес-ключ.
/// </summary>
public static class CalendarSessionTypeMapper
{
    private const string Category = MappingCategories.CalendarSessionType;

    public static CalendarSessionType Map(
        CalendarSessionTypeDTO dto,
        string market,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            _ = sourceTz;

            if (market is not ("stock" or "futures"))
            {
                throw new MappingValidationException(
                    $"{Category} requires market parameter 'stock' or 'futures' at row {rowIndex}, got '{market ?? "(null)"}'",
                    category: Category,
                    rowIndex: rowIndex);
            }

            if (string.IsNullOrEmpty(dto.Type))
            {
                throw new MappingValidationException(
                    $"{Category} requires non-empty Type at row {rowIndex}",
                    category: Category,
                    rowIndex: rowIndex);
            }

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{market}|{ctx.SourceCode}|{dto.Type}|{RowHashHelper.Fmt(dto.Title)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarSessionType
            {
                Market = market,
                Type = dto.Type,
                Title = dto.Title,

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

    public static List<CalendarSessionType> MapBatch(
        IReadOnlyList<CalendarSessionTypeDTO> dtos,
        string market,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarSessionType>(dtos.Count);
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
