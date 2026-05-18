using System.Diagnostics;
using System.Globalization;
using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

/// <summary>
/// Маппер FuturesSecurityDTO → IssFuturesSecurity. ISS, SecIdRequired (Lock §9).
/// Snapshot policy (Lock §6). Цены — double? → decimal? через explicit cast (Lock §7).
/// </summary>
public static class IssFuturesSecurityMapper
{
    private const string Category = MappingCategories.IssFuturesSecurity;

    public static IssFuturesSecurity Map(
        FuturesSecurityDTO dto,
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
                    $"IssFuturesSecurity DTO has invalid SECID at row {rowIndex}: '(null or empty)' (required)",
                    category: Category,
                    rowIndex: rowIndex);
            }

            DateOnly? lastTradeDate = dto.LASTTRADEDATE.HasValue
                ? DateOnly.FromDateTime(dto.LASTTRADEDATE.Value)
                : null;
            DateOnly? lastDelDate = dto.LASTDELDATE.HasValue
                ? DateOnly.FromDateTime(dto.LASTDELDATE.Value)
                : null;

            decimal? initialMargin = dto.INITIALMARGIN.HasValue ? (decimal)dto.INITIALMARGIN.Value : null;
            decimal? prevSettlePrice = dto.PREVSETTLEPRICE.HasValue ? (decimal)dto.PREVSETTLEPRICE.Value : null;
            decimal? minStep = dto.MINSTEP.HasValue ? (decimal)dto.MINSTEP.Value : null;
            decimal? highLimit = dto.HIGHLIMIT.HasValue ? (decimal)dto.HIGHLIMIT.Value : null;
            decimal? lowLimit = dto.LOWLIMIT.HasValue ? (decimal)dto.LOWLIMIT.Value : null;
            decimal? stepPrice = dto.STEPPRICE.HasValue ? (decimal)dto.STEPPRICE.Value : null;
            decimal? prevPrice = dto.PREVPRICE.HasValue ? (decimal)dto.PREVPRICE.Value : null;

            string canonical = string.Create(
                CultureInfo.InvariantCulture,
                $"{dto.SECID}|{ctx.SourceCode}|{RowHashHelper.Fmt(dto.SHORTNAME)}|{RowHashHelper.Fmt(dto.SECNAME)}|{RowHashHelper.Fmt(dto.ASSETCODE)}|{RowHashHelper.Fmt(initialMargin)}|{RowHashHelper.Fmt(prevSettlePrice)}|{RowHashHelper.Fmt(minStep)}|{RowHashHelper.Fmt(highLimit)}|{RowHashHelper.Fmt(lowLimit)}|{RowHashHelper.Fmt(stepPrice)}|{RowHashHelper.Fmt(prevPrice)}|{RowHashHelper.Fmt(dto.DECIMALS)}|{RowHashHelper.Fmt(dto.LOTVOLUME)}|{RowHashHelper.Fmt(dto.PREVOPENPOSITION)}|{RowHashHelper.Fmt(lastTradeDate)}|{RowHashHelper.Fmt(lastDelDate)}");

            ulong rowHash = RowHashHelper.Compute(canonical.AsSpan());

            return new IssFuturesSecurity
            {
                SecId = dto.SECID,
                ShortName = dto.SHORTNAME,
                SecName = dto.SECNAME,
                AssetCode = dto.ASSETCODE,
                InitialMargin = initialMargin,
                PrevSettlePrice = prevSettlePrice,
                MinStep = minStep,
                HighLimit = highLimit,
                LowLimit = lowLimit,
                StepPrice = stepPrice,
                PrevPrice = prevPrice,
                Decimals = dto.DECIMALS,
                LotVolume = dto.LOTVOLUME,
                PrevOpenPosition = dto.PREVOPENPOSITION,
                LastTradeDate = lastTradeDate,
                LastDelDate = lastDelDate,

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

    public static List<IssFuturesSecurity> MapBatch(
        IReadOnlyList<FuturesSecurityDTO> dtos,
        MapContext ctx,
        ILogger logger,
        CancellationToken ct = default)
    {
        long started = Stopwatch.GetTimestamp();
        MappingLogMessages.MapBatchStarted(logger, ctx.SourceCode, secId: string.Empty, Category, dtos.Count);

        var result = new List<IssFuturesSecurity>(dtos.Count);
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
