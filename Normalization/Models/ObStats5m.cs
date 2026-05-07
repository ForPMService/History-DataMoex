namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая 5-минутная статистика стакана заявок.
    /// </summary>
    public sealed record ObStats5m(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        double? Spread,
        double? ImbalanceVolume,
        double? ImbalanceValue,
        double? BidVolume,
        double? AskVolume,
        double? BidValue,
        double? AskValue,
        string SourceCode);
}