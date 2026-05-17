namespace History_DataMoex.RawStore.Errors;

/// <summary>
/// Базовый класс ошибок локального/объектного хранилища raw-объектов.
/// Не наследуется от MoexHttpException — другой слой, без HTTP-статуса и Retry-After.
/// </summary>
public abstract class RawStoreException : Exception
{
    public string? StoragePath { get; init; }
    public string? Sha256Hex { get; init; }
    public bool IsRetryable { get; init; }

    public abstract string ErrorCategory { get; }

    protected RawStoreException(string message, Exception? inner = null)
        : base(message, inner) { }
}
