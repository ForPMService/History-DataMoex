namespace History_DataMoex.Clients.Errors;

/// <summary>
/// Истекло время ожидания ответа от MOEX ISS API.
/// Бросается при <see cref="TaskCanceledException"/> или <see cref="TimeoutException"/>
/// во время HTTP-запроса; запрос может быть повторён.
/// </summary>
public sealed class MoexTimeoutException : MoexHttpException
{
    /// <summary>Настроенный таймаут запроса (null если не передан явно).</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Инициализирует новый экземпляр для ошибки таймаута.
    /// </summary>
    /// <param name="endpoint">Адрес эндпоинта, превысившего таймаут.</param>
    /// <param name="timeout">Настроенное время ожидания запроса.</param>
    /// <param name="inner">Исходное исключение таймаута (например, <see cref="TaskCanceledException"/>).</param>
    public MoexTimeoutException(string endpoint, TimeSpan? timeout = null, Exception? inner = null)
        : base($"MOEX request timed out for {endpoint}", inner)
    {
        Endpoint = endpoint;
        IsRetryable = true;
        Timeout = timeout;
    }
}
