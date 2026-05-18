using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarOptionsSeriesDTO → CalendarOptionsSeries.
/// Group C — SecId not applicable (Lock §9). Все поля nullable.
/// ExpirationDate — DateOnly?, ExpirationTime — TimeOnly?.
/// </summary>
public static class CalendarOptionsSeriesMapper
{
    private const string Category = MappingCategories.CalendarOptionsSeries;

    public static CalendarOptionsSeries Map(
        CalendarOptionsSeriesDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            _ = sourceTz;

            DateOnly? expirationDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.ExpirationDate, "expiration_date");
            TimeOnly? expirationTime = SourceDateTimeParser.ParseTimeOnlyOrNull(dto.ExpirationTime, "expiration_time");

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{RowHashHelper.Fmt(dto.AssetTypeName)}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.AssetCode)}|{RowHashHelper.Fmt(dto.SeriesName)}|{RowHashHelper.Fmt(dto.SeriesType)}|{RowHashHelper.Fmt(dto.ExecType)}|{RowHashHelper.Fmt(dto.MarginStyle)}|{RowHashHelper.Fmt(dto.ContractName)}|{RowHashHelper.Fmt(expirationDate)}|{RowHashHelper.Fmt(dto.ExpirationType)}|{RowHashHelper.Fmt(expirationTime)}|{RowHashHelper.Fmt(dto.WeekendSession)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarOptionsSeries
            {
                AssetTypeName = dto.AssetTypeName,
                AssetCode = dto.AssetCode,
                SeriesName = dto.SeriesName,
                SeriesType = dto.SeriesType,
                ExecType = dto.ExecType,
                MarginStyle = dto.MarginStyle,
                ContractName = dto.ContractName,
                ExpirationDate = expirationDate,
                ExpirationType = dto.ExpirationType,
                ExpirationTime = expirationTime,
                WeekendSession = dto.WeekendSession,

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

    public static List<CalendarOptionsSeries> MapBatch(
        IReadOnlyList<CalendarOptionsSeriesDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarOptionsSeries>(dtos.Count);
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
