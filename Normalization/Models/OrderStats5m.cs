namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая 5-минутная статистика выставленных и снятых заявок.
    /// </summary>
    public sealed record OrderStats5m(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        long? PutOrdersBuy,
        long? PutOrdersSell,
        long? CancelOrdersBuy,
        long? CancelOrdersSell,
        long? PutVolumeBuy,
        long? PutVolumeSell,
        long? CancelVolumeBuy,
        long? CancelVolumeSell,
        string SourceCode);
}