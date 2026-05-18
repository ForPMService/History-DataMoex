using System.Buffers;
using System.Globalization;
using System.IO.Hashing;
using System.Text;

namespace History_DataMoex.Mappers;

/// <summary>
/// Helper для вычисления RowHashV1.
/// Compute() избегает аллокаций byte[] на горячем пути:
/// stackalloc для коротких канонических строк (&lt;= 512 байт), ArrayPool для длинных.
/// Принцип фазы 4 (RentedBuffer): не плодить byte[] на потоке.
/// </summary>
public static class RowHashHelper
{
    private const int StackallocThreshold = 512;

    public static ulong Compute(ReadOnlySpan<char> canonical)
    {
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(canonical.Length);

        if (maxByteCount <= StackallocThreshold)
        {
            Span<byte> stackBytes = stackalloc byte[StackallocThreshold];
            int written = Encoding.UTF8.GetBytes(canonical, stackBytes);
            return XxHash64.HashToUInt64(stackBytes[..written]);
        }

        byte[] rented = ArrayPool<byte>.Shared.Rent(maxByteCount);
        try
        {
            int written = Encoding.UTF8.GetBytes(canonical, rented);
            return XxHash64.HashToUInt64(rented.AsSpan(0, written));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    public static string Fmt(double? v)
        => v.HasValue ? v.Value.ToString("G17", CultureInfo.InvariantCulture) : "";

    public static string Fmt(int? v)
        => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : "";

    public static string Fmt(long? v)
        => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : "";

    public static string Fmt(string? v)
        => v ?? "";

    /// <summary>
    /// Форматирует decimal? для канонической строки RowHashV1.
    /// G29 нормализует representation до значимых цифр: 1.0m и 1.00m дадут одинаковую строку.
    /// Без явного формата decimal.ToString() сохраняет scale → разные строки → разный hash → frozen V1 ломается.
    /// Lock §7.
    /// </summary>
    public static string Fmt(decimal? v)
        => v.HasValue ? v.Value.ToString("G29", CultureInfo.InvariantCulture) : "";

    /// <summary>
    /// Форматирует DateOnly? для канонической строки RowHashV1 как "yyyy-MM-dd".
    /// </summary>
    public static string Fmt(DateOnly? v)
        => v.HasValue ? v.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";

    /// <summary>
    /// Форматирует TimeOnly? для канонической строки RowHashV1 как "HH:mm:ss".
    /// </summary>
    public static string Fmt(TimeOnly? v)
        => v.HasValue ? v.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture) : "";

    /// <summary>
    /// Форматирует DateTime? UTC для канонической строки RowHashV1 через Ticks.
    /// Ticks — компактное целочисленное representation с гарантированной стабильностью между
    /// .NET версиями (в отличие от "O" round-trip формата, где могут быть нюансы микросекунд).
    /// Используется только для UTC timestamp-полей (UpdateTimeUtc, TimeFromUtc, и т.д.).
    /// Если DateTimeKind != Utc — это баг маппера; здесь проверка не делается (helper доверяет вызывающему).
    /// </summary>
    public static string FmtUtc(DateTime? v)
        => v.HasValue ? v.Value.Ticks.ToString(CultureInfo.InvariantCulture) : "";
}
