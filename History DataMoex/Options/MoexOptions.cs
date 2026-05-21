namespace History_DataMoex.Options
{
    /// <summary>
    /// Все настройки MOEX-источника в одном месте.
    /// ISS = публичный API (iss.moex.com), без ключа.
    /// APIM = платный API (apim.moex.com), с Bearer-токеном. Используется ALGOPACK и Calendar.
    /// </summary>
    public class MoexOptions
    {
        /// <summary>Base URL для ISS (без ключа). Default: https://iss.moex.com/iss</summary>
        public string IssBaseUrl { get; set; } = "https://iss.moex.com/iss";

        /// <summary>Base URL для APIM (ALGOPACK + Calendar). Default: https://apim.moex.com/iss</summary>
        public string ApimBaseUrl { get; set; } = "https://apim.moex.com/iss";

        /// <summary>Bearer-токен для APIM. Через user-secrets или переменные окружения.</summary>
        public string AlgKey { get; set; } = string.Empty;

        /// <summary>Таймаут одного HTTP-запроса.</summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public int MaxConnectionsPerServer { get; set; } = 32;
        public int MaxPagesPerLoad { get; set; } = 10_000;
    }
}
