namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая модель рыночного алерта.
    /// </summary>
    public sealed record Alert(
        Guid InstrumentId,
        DateTime EventTimeUtc,
        string AlertType,
        string? Severity,
        string? Description,
        string SourceCode);
}