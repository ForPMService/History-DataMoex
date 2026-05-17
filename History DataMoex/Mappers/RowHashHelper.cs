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
}
