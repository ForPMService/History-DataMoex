using History_DataMoex.Mappers.Errors;

namespace TestHistoryData.Phase7_5;

public class MappingErrorsTests
{
    [Fact]
    public void MappingValidationException_HasErrorCategory()
    {
        var ex = new MappingValidationException("invalid");
        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.False(ex.IsRetryable);
    }

    [Fact]
    public void MappingDateTimeException_HasErrorCategory_AndPreservesRawValues()
    {
        var ex = new MappingDateTimeException(
            "bad",
            rawTradeDate: "2026-99-99",
            rawTradeTime: "25:99:99");

        Assert.Equal("mapping_datetime", ex.ErrorCategory);
        Assert.False(ex.IsRetryable);
        Assert.Equal("2026-99-99", ex.RawTradeDate);
        Assert.Equal("25:99:99", ex.RawTradeTime);
    }

    [Fact]
    public void MappingTimezoneException_HasErrorCategory_AndIanaId()
    {
        var ex = new MappingTimezoneException("Invalid/Tz");

        Assert.Equal("mapping_timezone", ex.ErrorCategory);
        Assert.False(ex.IsRetryable);
        Assert.Equal("Invalid/Tz", ex.IanaId);
    }

    [Fact]
    public void MappingException_RowIndex_NullForBatchLevel_SetForRowLevel()
    {
        var rowEx = new MappingValidationException(
            "row-level",
            category: "candles",
            secId: "SBER",
            rowIndex: 42);
        Assert.Equal(42, rowEx.RowIndex);

        var batchEx = new MappingTimezoneException("Invalid/Tz");
        Assert.Null(batchEx.RowIndex);
        Assert.Null(batchEx.Category);
        Assert.Null(batchEx.SecId);
    }

    [Fact]
    public void WithContext_DoesNotOverwriteExistingValues()
    {
        var ex = new MappingValidationException(
            "msg",
            category: "a",
            secId: "X",
            rowIndex: 1);

        ex.WithContext("b", "Y", 9);

        Assert.Equal("a", ex.Category);
        Assert.Equal("X", ex.SecId);
        Assert.Equal(1, ex.RowIndex);
    }

    [Fact]
    public void WithContext_FillsNullFields()
    {
        var ex = new MappingDateTimeException("bad");
        Assert.Null(ex.Category);
        Assert.Null(ex.SecId);
        Assert.Null(ex.RowIndex);

        MappingException returned = ex.WithContext("candles", "SBER", 7);

        Assert.Same(ex, returned);
        Assert.Equal("candles", ex.Category);
        Assert.Equal("SBER", ex.SecId);
        Assert.Equal(7, ex.RowIndex);
    }
}
