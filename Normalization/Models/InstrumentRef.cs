namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническое описание торгового инструмента.
    /// Используется как внутренняя модель витрины, независимая от формата MOEX.
    /// </summary>
    public sealed record InstrumentRef(
        Guid InstrumentId,
        string DisplayCode,
        string SecId,
        string BoardId,
        string? AssetCode,
        string? Isin,
        InstrumentType Type);
}