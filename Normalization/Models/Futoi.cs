namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая модель открытого интереса по фьючерсам.
    /// </summary>
    public sealed record Futoi(
        Guid InstrumentId,
        DateTime BucketStartUtc,
        long? OpenInterest,
        long? OpenInterestChange,
        string SourceCode);
}