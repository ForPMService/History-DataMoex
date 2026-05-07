namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая 5-минутная статистика сделок.
    /// </summary>
    public sealed record TradeStats5m(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        double? PriceOpen,
        double? PriceHigh,
        double? PriceLow,
        double? PriceClose,
        double? PriceStd,
        double? Volume,
        double? Value,
        int? Trades,
        string SourceCode);
}