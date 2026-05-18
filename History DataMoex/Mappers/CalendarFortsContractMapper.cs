using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер CalendarFortsContractDTO → CalendarFortsContract.
/// Group A — SecIdRequired (Lock §9). ExpirationDate/EndDate — DateOnly? от string?.
/// ExpirationTime — TimeOnly? от string?. Без MSK→UTC полей.
/// </summary>
public static class CalendarFortsContractMapper
{
    private const string Category = MappingCategories.CalendarFortsContract;

    public static CalendarFortsContract Map(
        CalendarFortsContractDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            _ = sourceTz;

            if (string.IsNullOrEmpty(dto.SecId) || dto.SecId == "-")
            {
                throw new MappingValidationException(
                    $"{Category} DTO has invalid SecId at row {rowIndex}: '{dto.SecId ?? "(null)"}' (required, must be non-empty and not '-')",
                    category: Category,
                    secId: dto.SecId,
                    rowIndex: rowIndex);
            }

            DateOnly? expirationDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.ExpirationDate, "expiration_date");
            DateOnly? endDate = SourceDateTimeParser.ParseDateOnlyOrNull(dto.EndDate, "end_date");
            TimeOnly? expirationTime = SourceDateTimeParser.ParseTimeOnlyOrNull(dto.ExpirationTime, "expiration_time");

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{dto.SecId}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.AssetCode)}|{RowHashHelper.Fmt(dto.ShortName)}|{RowHashHelper.Fmt(dto.ExecType)}|{RowHashHelper.Fmt(dto.ContractName)}|{RowHashHelper.Fmt(expirationDate)}|{RowHashHelper.Fmt(endDate)}|{RowHashHelper.Fmt(dto.ExpirationType)}|{RowHashHelper.Fmt(expirationTime)}|{RowHashHelper.Fmt(dto.WeekendSession)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new CalendarFortsContract
            {
                SecId = dto.SecId,
                AssetCode = dto.AssetCode,
                ShortName = dto.ShortName,
                ExecType = dto.ExecType,
                ContractName = dto.ContractName,
                ExpirationDate = expirationDate,
                EndDate = endDate,
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
            ex.WithContext(Category, dto.SecId ?? string.Empty, rowIndex);
            throw;
        }
    }

    public static List<CalendarFortsContract> MapBatch(
        IReadOnlyList<CalendarFortsContractDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<CalendarFortsContract>(dtos.Count);
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
