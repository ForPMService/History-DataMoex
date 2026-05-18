namespace History_DataMoex.Mappers.Errors;

public sealed class MappingDateTimeException : MappingException
{
    public string? RawTradeDate { get; }
    public string? RawTradeTime { get; }

    // Поля для Calendar/ISS field-level date/time ошибок (Lock §14.2).
    // private set нужен, чтобы static factory ForField могла дозаполнить поля после
    // вызова existing constructor — constructor overload с такой сигнатурой
    // дал бы CS0111 (имена параметров и nullable annotations не входят в CLR signature).
    public string? FieldName { get; private set; }
    public string? RawValue { get; private set; }

    public MappingDateTimeException(
        string message,
        string? rawTradeDate = null,
        string? rawTradeTime = null,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        Exception? inner = null)
        : base(message, category, secId, rowIndex, isRetryable: false, inner: inner)
    {
        RawTradeDate = rawTradeDate;
        RawTradeTime = rawTradeTime;
        FieldName = null;
        RawValue = null;
    }

    /// <summary>
    /// Создаёт MappingDateTimeException для Calendar/ISS field-level date/time ошибок.
    /// Не constructor overload — потому что сигнатура совпала бы с existing ALGOPACK constructor
    /// (rawTradeDate/rawTradeTime → string?, string? — те же типы на CLR-уровне).
    /// Lock §14.2.
    /// </summary>
    public static MappingDateTimeException ForField(
        string message,
        string fieldName,
        string? rawValue,
        string? category = null,
        string? secId = null,
        int? rowIndex = null,
        Exception? inner = null)
    {
        // Используем existing ALGOPACK constructor c null для tradeDate/tradeTime;
        // FieldName/RawValue дозаполняем через private setter.
        var ex = new MappingDateTimeException(
            message,
            rawTradeDate: null,
            rawTradeTime: null,
            category: category,
            secId: secId,
            rowIndex: rowIndex,
            inner: inner);

        ex.FieldName = fieldName;
        ex.RawValue = rawValue;
        return ex;
    }

    public override string ErrorCategory => "mapping_datetime";
}
