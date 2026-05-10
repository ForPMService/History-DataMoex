namespace History_DataMoex.Options
{
    /// <summary>
    /// Базовые настройки HTTP-клиентов MOEX.
    /// Используются как общий контракт для ISS, ALGOPACK и будущих MOEX-клиентов.
    /// </summary>
    public abstract class MoexClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public string UserAgent { get; set; } = "HistoryDataMoex/0.1";
        public int MaxConnectionsPerServer { get; set; } = 32;



    }
}