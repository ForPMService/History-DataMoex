namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая модель HI2.
    /// </summary>
    public sealed record Hi2(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        double? Value,
        double? BuyValue,
        double? SellValue,
        string SourceCode);
}