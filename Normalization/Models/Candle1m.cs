namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая 1-минутная свеча.
    /// Время хранится в UTC.
    /// </summary>
    public sealed record Candle1m(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        double Open,
        double High,
        double Low,
        double Close,
        double Volume,
        double Value,
        string SourceCode);
}