namespace History_DataMoex.Clients.Errors;

/// <summary>
/// Тело ответа MOEX превысило допустимый размер после декомпрессии.
/// </summary>
public sealed class MoexResponseTooLargeException : MoexHttpException
{
    public long MaxResponseBytes { get; }

    public MoexResponseTooLargeException(long maxResponseBytes)
        : base($"MOEX response body exceeds the allowed size limit of {maxResponseBytes} bytes.")
    {
        MaxResponseBytes = maxResponseBytes;
        IsRetryable = false;
        SourceCode = "response_body";
    }

    public override string ErrorCategory => "response_too_large";
}