using History_DataMoex.Mappers.Errors;

namespace TestHistoryData.Phase8;

public class MappingExceptionPhase8Tests
{
    [Fact]
    public void WithContext_NoSecId_OverloadFillsCategoryAndRowIndex()
    {
        var ex = new MappingValidationException("invalid");
        ex.WithContext("calendar_offday_all", rowIndex: 5);

        Assert.Equal("calendar_offday_all", ex.Category);
        Assert.Equal(5, ex.RowIndex);
        Assert.Null(ex.SecId);
    }

    [Fact]
    public void WithContext_NoSecId_DoesNotOverwriteExistingSecId()
    {
        var ex = new MappingValidationException("invalid");
        ex.WithContext("a", "SBER", 1);
        ex.WithContext("b", 2);

        // Идемпотентность: исходные значения сохраняются, SecId не перезаписан.
        Assert.Equal("a", ex.Category);
        Assert.Equal("SBER", ex.SecId);
        Assert.Equal(1, ex.RowIndex);
    }

    [Fact]
    public void MappingDateTimeException_ForField_PreservesFieldNameAndRawValue()
    {
        MappingDateTimeException ex = MappingDateTimeException.ForField(
            "msg", fieldName: "expiration_date", rawValue: "bad");

        Assert.Equal("expiration_date", ex.FieldName);
        Assert.Equal("bad", ex.RawValue);
        Assert.Null(ex.RawTradeDate);
        Assert.Null(ex.RawTradeTime);
    }

    [Fact]
    public void MappingDateTimeException_LegacyConstructor_StillWorks()
    {
        var ex = new MappingDateTimeException(
            "msg", rawTradeDate: "2026-05-17", rawTradeTime: "10:00");

        Assert.Equal("2026-05-17", ex.RawTradeDate);
        Assert.Equal("10:00", ex.RawTradeTime);
        Assert.Null(ex.FieldName);
        Assert.Null(ex.RawValue);
    }
}
