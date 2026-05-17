namespace History_DataMoex.Mappers;

/// <summary>
/// Паспорт загрузки. Создаётся один раз при получении ответа от источника.
/// Передаётся в raw store и в маппер.
/// LoadJobId и RawObjectId передаются вызывающим кодом (Guid.CreateVersion7()).
/// </summary>
public sealed record MapContext(
    string SourceCode,        // "MOEX_ALGOPACK", "MOEX_ISS", "MOEX_CALENDAR"
    string Endpoint,          // нормализованный путь без query, например "/datashop/algopack/eq/candles/SBER.json"
    string SourceTimezone,    // IANA timezone id источника, "Europe/Moscow" для MOEX
    Guid LoadJobId,           // id загрузки
    Guid RawObjectId,         // id сырого объекта
    DateTime FetchedAtUtc);   // UTC timestamp момента получения ответа
