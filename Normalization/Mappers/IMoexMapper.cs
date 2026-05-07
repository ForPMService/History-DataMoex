namespace History_DataMoex.Normalization.Mappers
{
    /// <summary>
    /// Контракт будущего маппера из MOEX DTO в каноническую модель витрины.
    /// Реализации будут добавлены в треке TASK-IMPL-*.
    /// </summary>
    public interface IMoexMapper<TDto, TCanonical>
    {
        TCanonical Map(TDto dto, MapContext ctx);
    }
}