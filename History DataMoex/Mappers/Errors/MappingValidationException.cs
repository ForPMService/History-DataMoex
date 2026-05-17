namespace History_DataMoex.Mappers.Errors;

public sealed class MappingValidationException : MappingException
{
    public MappingValidationException(
        string message,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        Exception? inner = null)
        : base(message, category, secId, rowIndex, isRetryable: false, inner: inner)
    {
    }

    public override string ErrorCategory => "mapping_validation";
}
