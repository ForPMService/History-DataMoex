using History_DataMoex.Parsing;
using History_DataMoex.Parsing.Errors;
using System.Text;

namespace TestHistoryData.Phase8;

/// <summary>
/// Базовый набор тестов унификации parser structural errors (Lock §10).
/// Полное покрытие structural failures — этап 8-D.
/// </summary>
public class CalendarParserSchemaMismatchTests
{
    [Fact]
    public void ParseStockSessionWithTypes_MissingRootKey_ThrowsSchemaMismatch()
    {
        // JSON без корневого ключа "session_schedule"
        string json = """
        {
          "other_root": {
            "columns": ["x"],
            "data": []
          }
        }
        """;

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        Assert.Throws<MoexSchemaMismatchException>(
            () => ParsingCalendarUtf8.ParseStockSession(bytes));
    }

    [Fact]
    public void ParseOffDaysAll_DataBeforeColumns_ThrowsSchemaMismatch()
    {
        // data приходит до columns
        string json = """
        {
          "off_days": {
            "data": [
              ["2026-01-01", 0, null, "H", 0, null, "H", 0, null, "H"]
            ],
            "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"]
          }
        }
        """;

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        Assert.Throws<MoexSchemaMismatchException>(
            () => ParsingCalendarUtf8.ParseOffDaysAll(bytes));
    }

    [Fact]
    public void ParseFuturesSecuritiesAll_MissingDataBlock_ThrowsSchemaMismatch()
    {
        // forts с columns, но без data
        string json = """
        {
          "forts": {
            "columns": ["secid", "asset_code", "shortname", "exec_type", "contract_name", "expiration_date", "end_date", "expiration_type", "expiration_time", "weekend_session"]
          }
        }
        """;

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        Assert.Throws<MoexSchemaMismatchException>(
            () => ParsingCalendarUtf8.ParseFuturesSecurities(bytes));
    }

    [Fact]
    public void ParseSecurityChangesWithAttributes_MissingCursor_ThrowsSchemaMismatch()
    {
        // securities + securities.attributes есть, securities.cursor отсутствует
        string json = """
        {
          "securities": {
            "columns": ["updatetime", "action", "secid", "attribute_name", "before_value", "after_value"],
            "data": [
              ["2026-05-07 00:21:04", "updated", "RU000A0JXR84", "COUPONDATE", "2026-05-07", "2026-11-05"]
            ]
          },
          "securities.attributes": {
            "columns": ["name", "type", "title"],
            "data": [
              ["COUPONDATE", "D", "Дата выплаты купона"]
            ]
          }
        }
        """;

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        Assert.Throws<MoexSchemaMismatchException>(
            () => ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(bytes));
    }

    [Fact]
    public void ParseSuspendedWithReasons_MissingReasonsTable_ThrowsSchemaMismatch()
    {
        // Есть suspended, нет suspended.reasons
        string json = """
        {
          "suspended": {
            "columns": ["secid", "reason_id", "date_from", "date_till", "boardid", "settle_codes", "changedate", "updatetime"],
            "data": [
              ["AGNC-RM", "5002", "2026-01-05", null, "MPTR", "Y2-14", "2025-12-30", "2025-12-30 10:00:00"]
            ]
          },
          "suspended.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[0, 100, 100]]
          }
        }
        """;

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        Assert.Throws<MoexSchemaMismatchException>(
            () => ParsingCalendarUtf8.ParseSuspendedWithReasons(bytes));
    }
}
