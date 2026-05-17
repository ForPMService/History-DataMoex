namespace History_DataMoex.Mappers.Errors;

/// <summary>
/// Базовый класс ошибок mapping-слоя.
/// Поля Category/SecId/RowIndex — private set, обогащаются методом WithContext
/// из общего try/catch в Map(). Object initializer не используется (поля private set).
/// </summary>
public abstract class MappingException : Exception
{
    public string? Category { get; private set; }
    public string? SecId { get; private set; }
    public int? RowIndex { get; private set; }
    public bool IsRetryable { get; }

    public abstract string ErrorCategory { get; }

    protected MappingException(
        string message,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        bool isRetryable = false,
        Exception? inner = null)
        : base(message, inner)
    {
        Category = category;
        SecId = secId;
        RowIndex = rowIndex;
        IsRetryable = isRetryable;
    }

    /// <summary>
    /// Идемпотентно обогащает контекст: ??= не перезаписывает уже выставленные поля.
    /// Возвращает self для chaining: throw ex.WithContext(...).
    /// </summary>
    public MappingException WithContext(string category, string secId, int rowIndex)
    {
        Category ??= category;
        SecId ??= secId;
        RowIndex ??= rowIndex;
        return this;
    }
}
