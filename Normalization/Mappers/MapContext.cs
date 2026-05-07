namespace History_DataMoex.Normalization.Mappers
{
    /// <summary>
    /// Контекст нормализации одной строки данных.
    /// Передаёт сквозные идентификаторы будущего ingestion pipeline.
    /// </summary>
    public sealed record MapContext(
        Guid? InstrumentId,
        string SourceCode,
        string DataNeedCode,
        Guid? RawObjectId);
}