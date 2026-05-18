using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер StockSecurityDTO → IssStockSecurity. ISS, SecIdRequired (Lock §9).
/// Snapshot policy (Lock §6): FetchedAtUtc — момент загрузки snapshot.
/// FaceValue: double? → decimal? через explicit cast (Lock §7).
/// </summary>
public static class IssStockSecurityMapper
{
    private const string Category = MappingCategories.IssStockSecurity;

    public static IssStockSecurity Map(
        StockSecurityDTO dto,
        MapContext ctx,
        TimeZoneInfo sourceTz,
        int rowIndex = 0)
    {
        try
        {
            _ = sourceTz;

            if (string.IsNullOrEmpty(dto.SECID))
            {
                throw new MappingValidationException(
                    $"IssStockSecurity DTO has invalid SECID at row {rowIndex}: '(null or empty)' (required)",
                    category: Category,
                    rowIndex: rowIndex);
            }
            if (string.IsNullOrEmpty(dto.BOARDID))
            {
                throw new MappingValidationException(
                    $"IssStockSecurity DTO has invalid BOARDID at row {rowIndex}: '(null or empty)' (required)",
                    category: Category,
                    secId: dto.SECID,
                    rowIndex: rowIndex);
            }

            DateOnly? prevDate = dto.PREVDATE.HasValue
                ? DateOnly.FromDateTime(dto.PREVDATE.Value)
                : null;

            decimal? faceValue = dto.FACEVALUE.HasValue
                ? (decimal)dto.FACEVALUE.Value
                : null;

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{dto.SECID}|{dto.BOARDID}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.SHORTNAME)}|{RowHashHelper.Fmt(dto.SECNAME)}|{RowHashHelper.Fmt(dto.MARKETCODE)}|{RowHashHelper.Fmt(dto.PREVLEGALCLOSEPRICE)}|{RowHashHelper.Fmt(dto.LOTSIZE)}|{RowHashHelper.Fmt(faceValue)}|{RowHashHelper.Fmt(prevDate)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new IssStockSecurity
            {
                SecId = dto.SECID,
                BoardId = dto.BOARDID,
                ShortName = dto.SHORTNAME,
                SecName = dto.SECNAME,
                MarketCode = dto.MARKETCODE,
                PrevLegalClosePrice = dto.PREVLEGALCLOSEPRICE,
                LotSize = dto.LOTSIZE,
                FaceValue = faceValue,
                PrevDate = prevDate,

                Source = ctx.SourceCode,
                RowHashV1 = rowHash,
                RawObjectId = ctx.RawObjectId,
                LoadJobId = ctx.LoadJobId,
                FetchedAtUtc = ctx.FetchedAtUtc,
            };
        }
        catch (MappingException ex)
        {
            ex.WithContext(Category, dto.SECID ?? string.Empty, rowIndex);
            throw;
        }
    }

    public static List<IssStockSecurity> MapBatch(
        IReadOnlyList<StockSecurityDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<IssStockSecurity>(dtos.Count);
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
