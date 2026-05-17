namespace History_DataMoex.Mappers.Errors;

public sealed class MappingDateTimeException : MappingException
{
    public string? RawTradeDate { get; }
    public string? RawTradeTime { get; }

    public MappingDateTimeException(
        string message,
        string? rawTradeDate = null,
        string? rawTradeTime = null,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        Exception? inner = null)
        : base(message, category, secId, rowIndex, isRetryable: false, inner: inner)
    {
        RawTradeDate = rawTradeDate;
        RawTradeTime = rawTradeTime;
    }

    public override string ErrorCategory => "mapping_datetime";
}
