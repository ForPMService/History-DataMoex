using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarSecurityChangeDTO → CalendarSecurityChange.
/// Group A — SecIdRequired (Lock §9). UpdateTimeUtc — required бизнес-ключ (Lock §4).
/// </summary>
public static class CalendarSecurityChangeMapper
{
    private const string Category = MappingCategories.CalendarSecurityChange;

    public static CalendarSecurityChange Map(
        CalendarSecurityChangeDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.SecId) || dto.SecId == "-")
            {
                throw new MappingValidationException(
                    $"{Category} DTO has invalid SecId at row {rowIndex}: '{dto.SecId ?? "(null)"}' (required, must be non-empty and not '-')",
                    category: Category,
                    secId: dto.SecId,
                    rowIndex: rowIndex);
            }

            DateTime? updateTimeUtcMaybe = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(
                dto.UpdateTime, sourceTz, "update_time");
            DateTime updateTimeUtc = updateTimeUtcMaybe
                ?? throw MappingDateTimeException.ForField(
                    $"Required field 'update_time' is null at row {rowIndex} (бизнес-ключ)",
                    fieldName: "update_time",
                    rawValue: null,
                    category: Category,
                    secId: dto.SecId,
                    rowIndex: rowIndex);

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{RowHashHelper.FmtUtc((DateTime?)updateTimeUtc)}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.Action)}|{dto.SecId}|{RowHashHelper.Fmt(dto.AttributeName)}|{RowHashHelper.Fmt(dto.BeforeValue)}|{RowHashHelper.Fmt(dto.AfterValue)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarSecurityChange
            {
                UpdateTimeUtc = updateTimeUtc,
                Action = dto.Action,
                SecId = dto.SecId,
                AttributeName = dto.AttributeName,
                BeforeValue = dto.BeforeValue,
                AfterValue = dto.AfterValue,

                Source = ctx.SourceCode,
                RowHashV1 = rowHash,
                RawObjectId = ctx.RawObjectId,
                LoadJobId = ctx.LoadJobId,
                FetchedAtUtc = ctx.FetchedAtUtc,
            };
        }
        catch (MappingException ex)
        {
            ex.WithContext(Category, dto.SecId ?? string.Empty, rowIndex);
            throw;
        }
    }

    public static List<CalendarSecurityChange> MapBatch(
        IReadOnlyList<CalendarSecurityChangeDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarSecurityChange>(dtos.Count);
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
