namespace History_DataMoex.Clients
{
    /// <summary>
    /// Ошибка HTTP-взаимодействия с MOEX.
    /// Пока не используется в клиентах напрямую.
    /// Нужна как общий контракт для будущей обработки ошибок, логирования и ingestion jobs.
    /// </summary>
    public class MoexHttpException : Exception
    {
        public string? SourceCode { get; init; }

        public int? StatusCode { get; init; }

        public string? Endpoint { get; init; }

        public string? CorrelationId { get; init; }

        public MoexHttpException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }
    }
}