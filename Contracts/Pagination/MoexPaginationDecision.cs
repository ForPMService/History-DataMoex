namespace History_DataMoex.Contracts.Pagination;

/// <summary>
/// Единая точка принятия решения: нужна ли следующая страница MOEX.
/// Пока клиенты на этот helper не переводятся; он нужен для следующего трека реализации.
/// </summary>
public static class MoexPaginationDecision
{
    public static MoexPageRequest? Next<T>(MoexPageResult<T> previous)
    {
        return previous.Kind switch
        {
            MoexPaginationKind.Cursor => NextCursor(previous),
            MoexPaginationKind.FixedPage500 => NextFixed(previous, 500),
            MoexPaginationKind.FixedPage1000 => NextFixed(previous, 1000),
            _ => null
        };
    }

    private static MoexPageRequest? NextCursor<T>(MoexPageResult<T> previous)
    {
        if (!previous.Index.HasValue ||
            !previous.Total.HasValue ||
            !previous.PageSize.HasValue)
        {
            return null;
        }

        int nextStart = previous.Index.Value + previous.PageSize.Value;

        if (nextStart >= previous.Total.Value)
        {
            return null;
        }

        return new MoexPageRequest(nextStart, previous.Kind);
    }

    private static MoexPageRequest? NextFixed<T>(
        MoexPageResult<T> previous,
        int pageSize)
    {
        if (previous.Rows.Count < pageSize)
        {
            return null;
        }

        int currentStart = previous.Index ?? 0;
        int nextStart = currentStart + pageSize;

        return new MoexPageRequest(nextStart, previous.Kind);
    }
}