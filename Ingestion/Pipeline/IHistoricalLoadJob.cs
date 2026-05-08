using History_DataMoex.Ingestion.Models;

namespace History_DataMoex.Ingestion.Pipeline;

/// <summary>
/// Воркер фоновой загрузки данных MOEX в БД. Реализуется в фазе 2.
/// Алгоритм: создание задачи → сохранение исходного JSON → парсинг →
/// дописать поля происхождения загрузки → запись в ClickHouse → обновить статус задачи.
/// Ручки витрины под /api/v1/* проектируются после реализации этого воркера, не до.
/// </summary>
public interface IHistoricalLoadJob
{
    Task<HistoricalLoadResult> ExecuteAsync(HistoricalLoadRequest request, CancellationToken ct);
}