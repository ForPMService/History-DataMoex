namespace History_DataMoex.Mappers.Errors;

public sealed class MappingTimezoneException : MappingException
{
    public string IanaId { get; }

    public MappingTimezoneException(
        string ianaId,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        Exception? inner = null)
        : base(
            $"Source timezone '{ianaId}' could not be resolved and has no fallback",
            category, secId, rowIndex, isRetryable: false, inner: inner)
    {
        IanaId = ianaId;
    }

    public override string ErrorCategory => "mapping_timezone";
}
