using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarSuspendedDTO → CalendarSuspension.
/// Group A — SecIdRequired (Lock §9). UpdateTime → UpdateTimeUtc через MSK→UTC (Lock §4).
/// ReasonId парсится из string? в int? через int.TryParse — MOEX отдаёт reason_id как string.
/// </summary>
public static class CalendarSuspensionMapper
{
    private const string Category = MappingCategories.CalendarSuspension;

    public static CalendarSuspension Map(
        CalendarSuspendedDTO dto,
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

            int? reasonId = null;
            if (!string.IsNullOrEmpty(dto.ReasonId))
            {
                if (!int.TryParse(dto.ReasonId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedReasonId))
                {
                    throw new MappingValidationException(
                        $"{Category} DTO has invalid ReasonId at row {rowIndex}: '{dto.ReasonId}' (expected int string)",
                        category: Category,
                        secId: dto.SecId,
                        rowIndex: rowIndex);
                }
                reasonId = parsedReasonId;
            }

            DateOnly? dateFrom = SourceDateTimeParser.ParseDateOnlyOrNull(dto.DateFrom, "date_from");
            DateOnly? dateTill = SourceDateTimeParser.ParseDateOnlyOrNull(dto.DateTill, "date_till");
            DateOnly? changeDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.ChangeDate, "change_date");
            DateTime? updateTimeUtc = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(dto.UpdateTime, sourceTz, "update_time");

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{dto.SecId}|{ctx.SourceCode}|{RowHashHelper.Fmt(reasonId)}|{RowHashHelper.Fmt(dateFrom)}|{RowHashHelper.Fmt(dateTill)}|{RowHashHelper.Fmt(dto.BoardId)}|{RowHashHelper.Fmt(dto.SettleCodes)}|{RowHashHelper.Fmt(changeDate)}|{RowHashHelper.FmtUtc(updateTimeUtc)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarSuspension
            {
                SecId = dto.SecId,
                ReasonId = reasonId,
                DateFrom = dateFrom,
                DateTill = dateTill,
                BoardId = dto.BoardId,
                SettleCodes = dto.SettleCodes,
                ChangeDate = changeDate,
                UpdateTimeUtc = updateTimeUtc,

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

    public static List<CalendarSuspension> MapBatch(
        IReadOnlyList<CalendarSuspendedDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarSuspension>(dtos.Count);
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
