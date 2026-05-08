namespace History_DataMoex.Ingestion.Models;

public sealed record ParsedBatch<TDto>(
    LoadContext Context,
    IReadOnlyList<TDto> Rows,
    string PaginationKind);