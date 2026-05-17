using System.Globalization;
using System.IO.Hashing;
using System.Text;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Models;

namespace History_DataMoex.Mappers;

public static class CandlesMapper
{
    /// <summary>
    /// Маппинг одной CandlesDTO → Candle1m.
    /// TimeZoneInfo передаётся снаружи — не резолвить на каждую свечу.
    /// </summary>
    public static Candle1m Map(CandlesDTO dto, string secId, MapContext ctx, TimeZoneInfo sourceTz)
    {
        DateTime beginLocal = dto.Begin
            ?? throw new InvalidOperationException("CandlesDTO.Begin is null");

        DateTime beginUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(beginLocal, DateTimeKind.Unspecified), sourceTz);

        ulong rowHash = ComputeRowHash(secId, beginUtc, 60, ctx.SourceCode,
            dto.Open, dto.High, dto.Low, dto.Close, dto.Volume, dto.Value);

        return new Candle1m
        {
            SecId = secId,
            BeginUtc = beginUtc,
            BeginLocal = beginLocal,
            IntervalSeconds = 60,
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

    /// <summary>
    /// Batch-маппинг. TimeZoneInfo резолвится ОДИН РАЗ из ctx.SourceTimezone.
    /// </summary>
    public static List<Candle1m> MapBatch(IReadOnlyList<CandlesDTO> dtos, string secId, MapContext ctx)
    {
        TimeZoneInfo sourceTz = TimeZoneInfo.FindSystemTimeZoneById(ctx.SourceTimezone);
        var result = new List<Candle1m>(dtos.Count);
        foreach (CandlesDTO dto in dtos)
        {
            result.Add(Map(dto, secId, ctx, sourceTz));
        }
        return result;
    }

    /// <summary>
    /// xxHash64 от строки идентификации записи.
    ///
    /// Формат:
    ///   {secId}|{beginUtcTicks}|{intervalSeconds}|{source}|{open}|{high}|{low}|{close}|{volume}|{value}
    ///
    /// Правила:
    ///   - beginUtcTicks = DateTime.Ticks (long), детерминированный
    ///   - double: InvariantCulture, формат G17 (полная точность IEEE 754)
    ///   - null → пустая строка
    ///   - разделитель | — не встречается в значениях
    /// </summary>
    private static ulong ComputeRowHash(
        string secId, DateTime beginUtc, int intervalSeconds, string source,
        double? open, double? high, double? low, double? close,
        double? volume, double? value)
    {
        string canonical = string.Create(CultureInfo.InvariantCulture,
            $"{secId}|{beginUtc.Ticks}|{intervalSeconds}|{source}|" +
            $"{Fmt(open)}|{Fmt(high)}|{Fmt(low)}|{Fmt(close)}|" +
            $"{Fmt(volume)}|{Fmt(value)}");

        return XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(canonical));
    }

    private static string Fmt(double? v)
        => v.HasValue ? v.Value.ToString("G17", CultureInfo.InvariantCulture) : "";
}
