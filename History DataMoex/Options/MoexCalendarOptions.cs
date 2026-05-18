namespace History_DataMoex.Options
{
    /// <summary>
    /// Configuration contract для Calendar source (MOEX ISS Calendar endpoints).
    /// Наследует BaseUrl, RequestTimeout, UserAgent, MaxConnectionsPerServer, MaxPagesPerLoad
    /// от MoexClientOptions. Добавляет только Key — токен авторизации для Calendar API.
    ///
    /// Контракт разделён с MoexAlgOptions (ALGOPACK): даже если BaseUrl/Key совпадают сегодня,
    /// при переносе в ProjectTraiding Calendar становится независимым DataSource adapter.
    /// Lock §13.
    /// </summary>
    public class MoexCalendarOptions : MoexClientOptions
    {
        public string Key { get; set; } = string.Empty;
    }
}
