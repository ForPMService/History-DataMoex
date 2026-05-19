using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Parsing;
using History_DataMoex.Parsing.Errors;
using System.Text;

namespace TestHistoryData.Phase8;

/// <summary>
/// Расширение базовых parser contract тестов (8-A): edge cases — пустые data,
/// all-null rows, пустые вторичные таблицы, пустой cursor, негативные индексы,
/// homogeneous structural failure scenarios (Theory).
/// Lock §10 (parser errors), Lock §15 (rows policy).
/// </summary>
public class Phase8ParserContractTests
{
    // ════════════════════════════════════════════════════════════
    // Edge cases — single-table parsers
    // ════════════════════════════════════════════════════════════

    [Fact]
    public void ParseOffDaysAll_EmptyDataArray_ReturnsEmptyList()
    {
        const string json = """
        {
          "off_days": {
            "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"],
            "data": []
          }
        }
        """;

        List<CalendarOffDaysAllDTO> result = ParsingCalendarUtf8.ParseOffDaysAll(Encoding.UTF8.GetBytes(json));

        Assert.Empty(result);
    }

    [Fact]
    public void ParseOffDaysAll_AllNullsInRow_PreservesNulls()
    {
        const string json = """
        {
          "off_days": {
            "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"],
            "data": [
              [null, null, null, null, null, null, null, null, null, null]
            ]
          }
        }
        """;

        List<CalendarOffDaysAllDTO> result = ParsingCalendarUtf8.ParseOffDaysAll(Encoding.UTF8.GetBytes(json));

        Assert.Single(result);
        CalendarOffDaysAllDTO dto = result[0];
        Assert.Null(dto.TradeDate);
        Assert.Null(dto.CurrencyWorkday);
        Assert.Null(dto.CurrencyTradeSessionDate);
        Assert.Null(dto.CurrencyReason);
        Assert.Null(dto.FuturesWorkday);
        Assert.Null(dto.StockReason);
    }

    [Fact]
    public void ParseOffDaysMarket_ValidData_ReturnsList()
    {
        const string json = """
        {
          "off_days": {
            "columns": ["tradedate", "is_traded", "trade_session_date", "reason", "updatetime"],
            "data": [
              ["2026-05-09", 0, "2026-05-08", "H", "2026-05-08 18:00:00"]
            ]
          }
        }
        """;

        List<CalendarOffDaysMarketDTO> result = ParsingCalendarUtf8.ParseOffDaysMarket(Encoding.UTF8.GetBytes(json));

        Assert.Single(result);
        Assert.Equal("2026-05-09", result[0].TradeDate);
        Assert.Equal(0, result[0].IsTraded);
        Assert.Equal("H", result[0].Reason);
    }

    // ════════════════════════════════════════════════════════════
    // Edge cases — multi-table с cursor
    // ════════════════════════════════════════════════════════════

    [Fact]
    public void ParseSuspendedWithReasons_EmptySuspendedTable_StillReturnsReasonsAndCursor()
    {
        const string json = """
        {
          "suspended": {
            "columns": ["secid", "reason_id", "date_from", "date_till", "boardid", "settle_codes", "changedate", "updatetime"],
            "data": []
          },
          "suspended.reasons": {
            "columns": ["id", "title"],
            "data": [
              [1, "Reason A"],
              [2, "Reason B"]
            ]
          },
          "suspended.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[0, 0, 100]]
          }
        }
        """;

        var (suspended, reasons, cursor) =
            ParsingCalendarUtf8.ParseSuspendedWithReasons(Encoding.UTF8.GetBytes(json));

        Assert.Empty(suspended);
        Assert.Equal(2, reasons.Count);
        Assert.Equal(0, cursor.Index);
        Assert.Equal(0, cursor.Total);
        Assert.Equal(100, cursor.PageSize);
    }

    [Fact]
    public void ParseSuspendedWithReasons_EmptyCursorAllNulls_ReturnsCursorWithNullFields()
    {
        const string json = """
        {
          "suspended": {
            "columns": ["secid", "reason_id", "date_from", "date_till", "boardid", "settle_codes", "changedate", "updatetime"],
            "data": []
          },
          "suspended.reasons": {
            "columns": ["id", "title"],
            "data": []
          },
          "suspended.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[null, null, null]]
          }
        }
        """;

        var (_, _, cursor) = ParsingCalendarUtf8.ParseSuspendedWithReasons(Encoding.UTF8.GetBytes(json));

        Assert.Null(cursor.Index);
        Assert.Null(cursor.Total);
        Assert.Null(cursor.PageSize);
    }

    [Fact]
    public void ParseSecurityChangesWithAttributes_EmptyChangesAndAttributes_ReturnsBoth()
    {
        const string json = """
        {
          "securities": {
            "columns": ["updatetime", "action", "secid", "attribute_name", "before_value", "after_value"],
            "data": []
          },
          "securities.attributes": {
            "columns": ["name", "type", "title"],
            "data": []
          },
          "securities.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[0, 0, 100]]
          }
        }
        """;

        var (changes, attributes, cursor) =
            ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(Encoding.UTF8.GetBytes(json));

        Assert.Empty(changes);
        Assert.Empty(attributes);
        Assert.Equal(0, cursor.Index);
    }

    // ════════════════════════════════════════════════════════════
    // Edge cases — multi-table без cursor
    // ════════════════════════════════════════════════════════════

    [Fact]
    public void ParseFuturesSecurities_EmptyForts_ReturnsOptionsOnly()
    {
        const string json = """
        {
          "forts": {
            "columns": ["secid", "asset_code", "shortname", "exec_type", "contract_name", "expiration_date", "end_date", "expiration_type", "expiration_time", "weekend_session"],
            "data": []
          },
          "options": {
            "columns": ["asset_type_name", "asset_code", "series_name", "series_type", "exec_type", "margin_style", "contract_name", "expiration_date", "expiration_type", "expiration_time", "weekend_session"],
            "data": [
              ["Currency", "Si", "Si-6.26M", "Standard", "FX", "Future", "USD/RUB Opt June 2026", "2026-06-18", "Standard", "18:45:00", 0]
            ]
          }
        }
        """;

        var (forts, options) = ParsingCalendarUtf8.ParseFuturesSecurities(Encoding.UTF8.GetBytes(json));

        Assert.Empty(forts);
        Assert.Single(options);
        Assert.Equal("Si-6.26M", options[0].SeriesName);
    }

    [Fact]
    public void ParseStockSession_EmptyTypesTable_StillReturnsSessions()
    {
        const string json = """
        {
          "session_schedule": {
            "columns": ["tradedate", "tradingsession", "boardid", "secid", "type", "time_from", "time_till", "updatetime"],
            "data": [
              ["2026-05-17", 2, "TQBR", "-", "MAIN", "10:00:00", "18:45:00", "2026-05-17 09:00:00"]
            ]
          },
          "session_schedule.types": {
            "columns": ["type", "title"],
            "data": []
          }
        }
        """;

        var (sessions, types) = ParsingCalendarUtf8.ParseStockSession(Encoding.UTF8.GetBytes(json));

        Assert.Single(sessions);
        Assert.Empty(types);
        Assert.Equal(2, sessions[0].TradingSession);
    }

    [Fact]
    public void ParseFuturesSession_HappyPath_ReturnsBothTables()
    {
        const string json = """
        {
          "session_schedule": {
            "columns": ["trade_session_date", "boardid", "secid", "type", "time_from", "time_till", "updatetime"],
            "data": [
              ["2026-05-17", "RFUD", "-", "MAIN", "2026-05-17 10:00:00", "2026-05-17 18:50:00", "2026-05-17 09:00:00"]
            ]
          },
          "session_schedule.types": {
            "columns": ["type", "title"],
            "data": [
              ["MAIN", "Основная сессия"]
            ]
          }
        }
        """;

        var (sessions, types) = ParsingCalendarUtf8.ParseFuturesSession(Encoding.UTF8.GetBytes(json));

        Assert.Single(sessions);
        Assert.Single(types);
        Assert.Equal("MAIN", types[0].Type);
    }

    // ════════════════════════════════════════════════════════════
    // ISS edge cases — nullable значения
    // ════════════════════════════════════════════════════════════

    [Fact]
    public void ParseIssSecurityStock_SecidNullInRow_DtoSecidIsNull()
    {
        // 27 колонок; используем 9 из них (по SourceIndex 0,1,2,4,5,9,11,17,22).
        // Пишем null для SECID (позиция 0), остальное минимальное valid.
        const string json = """
        {
          "securities": {
            "columns": ["SECID","BOARDID","SHORTNAME","x3","LOTSIZE","FACEVALUE","x6","x7","x8","SECNAME","x10","MARKETCODE","x12","x13","x14","x15","x16","PREVDATE","x18","x19","x20","x21","PREVLEGALCLOSEPRICE","x23","x24","x25","x26"],
            "data": [
              [null, "TQBR", "Short", null, 10, 3.0, null, null, null, "Full", null, "FNDT", null, null, null, null, null, "2026-05-17", null, null, null, null, 290.5, null, null, null, null]
            ]
          }
        }
        """;

        List<StockSecurityDTO> result = ParsingIssUtf8.ParseIssSecurityStock(Encoding.UTF8.GetBytes(json));

        Assert.Single(result);
        Assert.Null(result[0].SECID);
        Assert.Equal("TQBR", result[0].BOARDID);
    }

    [Fact]
    public void ParseIssSecurityFutures_AllNumericFieldsNull_DtoNumericsAllNull()
    {
        // 26 колонок; numeric поля все null. SECID присутствует.
        const string json = """
        {
          "securities": {
            "columns": ["SECID","x1","SHORTNAME","SECNAME","PREVSETTLEPRICE","DECIMALS","MINSTEP","LASTTRADEDATE","LASTDELDATE","x9","x10","ASSETCODE","PREVOPENPOSITION","LOTVOLUME","INITIALMARGIN","HIGHLIMIT","LOWLIMIT","STEPPRICE","x18","PREVPRICE","x20","x21","x22","x23","x24","x25"],
            "data": [
              ["SiM6", null, "Si-6.26", "USD/RUB", null, null, null, null, null, null, null, "Si", null, null, null, null, null, null, null, null, null, null, null, null, null, null]
            ]
          }
        }
        """;

        List<FuturesSecurityDTO> result = ParsingIssUtf8.ParseIssSecurityFutures(Encoding.UTF8.GetBytes(json));

        Assert.Single(result);
        FuturesSecurityDTO dto = result[0];
        Assert.Equal("SiM6", dto.SECID);
        Assert.Null(dto.PREVSETTLEPRICE);
        Assert.Null(dto.MINSTEP);
        Assert.Null(dto.HIGHLIMIT);
        Assert.Null(dto.LOWLIMIT);
        Assert.Null(dto.PREVPRICE);
        Assert.Null(dto.PREVOPENPOSITION);
        Assert.Null(dto.LASTTRADEDATE);
    }

    // ════════════════════════════════════════════════════════════
    // Cursor parser edge cases
    // ════════════════════════════════════════════════════════════

    [Fact]
    public void ParseCursorUtf8_MissingPageSizeViaJsonNull_TreatedAsNull()
    {
        // Cursor с null в позиции PAGESIZE (3-я колонка).
        const string json = """
        {
          "data.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[0, 100, null]]
          }
        }
        """;

        PaginationCursorDTO cursor = ParseHelpersUtf8.ParseCursorUtf8(
            Encoding.UTF8.GetBytes(json), "data.cursor");

        Assert.Equal(0, cursor.Index);
        Assert.Equal(100, cursor.Total);
        Assert.Null(cursor.PageSize);
    }

    [Fact]
    public void ParseCursorUtf8_NegativeIndex_PreservedAsValue()
    {
        const string json = """
        {
          "data.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[-1, 100, 100]]
          }
        }
        """;

        PaginationCursorDTO cursor = ParseHelpersUtf8.ParseCursorUtf8(
            Encoding.UTF8.GetBytes(json), "data.cursor");

        Assert.Equal(-1, cursor.Index);
        Assert.Equal(100, cursor.Total);
    }

    // ════════════════════════════════════════════════════════════
    // Theory: structural failures (Lock §10) — все Calendar и ISS парсеры
    // ════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("OffDaysAll", """{"off_days":{"data":[["2026-05-09",0,null,"H",0,null,"H",0,null,"H"]],"columns":["tradedate","currency_workday","currency_trade_session_date","currency_reason","futures_workday","futures_trade_session_date","futures_reason","stock_workday","stock_trade_session_date","stock_reason"]}}""")]
    [InlineData("OffDaysMarket", """{"off_days":{"data":[["2026-05-09",0,"2026-05-08","H","2026-05-08 18:00:00"]],"columns":["tradedate","is_traded","trade_session_date","reason","updatetime"]}}""")]
    [InlineData("StockSession", """{"session_schedule":{"data":[["2026-05-17",2,"TQBR","-","MAIN","10:00:00","18:45:00","2026-05-17 09:00:00"]],"columns":["tradedate","tradingsession","boardid","secid","type","time_from","time_till","updatetime"]}}""")]
    [InlineData("FuturesSession", """{"session_schedule":{"data":[["2026-05-17","RFUD","-","MAIN","2026-05-17 10:00:00","2026-05-17 18:50:00","2026-05-17 09:00:00"]],"columns":["trade_session_date","boardid","secid","type","time_from","time_till","updatetime"]}}""")]
    [InlineData("FuturesSecurities", """{"forts":{"data":[["SiM6","Si","Si-6.26","FX","USD/RUB","2026-06-18","2026-06-18","Standard","18:45:00",0]],"columns":["secid","asset_code","shortname","exec_type","contract_name","expiration_date","end_date","expiration_type","expiration_time","weekend_session"]}}""")]
    [InlineData("SuspendedWithReasons", """{"suspended":{"data":[["SBER","1","2026-01-05",null,"TQBR","Y2","2025-12-30","2025-12-30 10:00:00"]],"columns":["secid","reason_id","date_from","date_till","boardid","settle_codes","changedate","updatetime"]}}""")]
    [InlineData("SecurityChangesWithAttributes", """{"securities":{"data":[["2026-05-07 00:21:04","updated","SBER","LOTSIZE","10","1"]],"columns":["updatetime","action","secid","attribute_name","before_value","after_value"]}}""")]
    public void ParsingCalendarUtf8_DataBeforeColumns_AllParsers_ThrowSchemaMismatch(string parser, string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var invocation = new ParserInvocation(parser, bytes);

        Assert.Throws<MoexSchemaMismatchException>(invocation.InvokeCalendarParser);
    }

    [Theory]
    [InlineData("Stock", """{"securities":{"data":[["SBER","TQBR","Short",null,10,3.0,null,null,null,"Full",null,"FNDT",null,null,null,null,null,"2026-05-17",null,null,null,null,290.5,null,null,null,null]],"columns":["SECID","BOARDID","SHORTNAME","x3","LOTSIZE","FACEVALUE","x6","x7","x8","SECNAME","x10","MARKETCODE","x12","x13","x14","x15","x16","PREVDATE","x18","x19","x20","x21","PREVLEGALCLOSEPRICE","x23","x24","x25","x26"]}}""")]
    [InlineData("Futures", """{"securities":{"data":[["SiM6",null,"Si-6.26","USD/RUB",92450.0,0,1.0,"2026-06-18","2026-06-19",null,null,"Si",250000,1000,5000.0,95000.0,90000.0,1.0,null,92500.0,null,null,null,null,null,null]],"columns":["SECID","x1","SHORTNAME","SECNAME","PREVSETTLEPRICE","DECIMALS","MINSTEP","LASTTRADEDATE","LASTDELDATE","x9","x10","ASSETCODE","PREVOPENPOSITION","LOTVOLUME","INITIALMARGIN","HIGHLIMIT","LOWLIMIT","STEPPRICE","x18","PREVPRICE","x20","x21","x22","x23","x24","x25"]}}""")]
    public void ParsingIssUtf8_DataBeforeColumns_AllParsers_ThrowSchemaMismatch(string parser, string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var invocation = new ParserInvocation(parser, bytes);

        Assert.Throws<MoexSchemaMismatchException>(invocation.InvokeIssParser);
    }

    [Theory]
    [InlineData("OffDaysAll", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("OffDaysMarket", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("StockSession", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("FuturesSession", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("FuturesSecurities", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("SuspendedWithReasons", """{"other_root":{"columns":["x"],"data":[]}}""")]
    [InlineData("SecurityChangesWithAttributes", """{"other_root":{"columns":["x"],"data":[]}}""")]
    public void ParsingCalendarUtf8_MissingRootKey_AllParsers_ThrowSchemaMismatch(string parser, string json)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var invocation = new ParserInvocation(parser, bytes);

        Assert.Throws<MoexSchemaMismatchException>(invocation.InvokeCalendarParser);
    }

    // ════════════════════════════════════════════════════════════
    // Helpers
    // ════════════════════════════════════════════════════════════

    private sealed class ParserInvocation
    {
        private readonly string _parser;
        private readonly byte[] _bytes;

        public ParserInvocation(string parser, byte[] bytes)
        {
            _parser = parser;
            _bytes = bytes;
        }

        public void InvokeCalendarParser()
        {
            Phase8ParserContractTests.InvokeCalendarParser(_parser, _bytes);
        }

        public void InvokeIssParser()
        {
            Phase8ParserContractTests.InvokeIssParser(_parser, _bytes);
        }
    }

    private static void InvokeCalendarParser(string parser, byte[] bytes)
    {
        switch (parser)
        {
            case "OffDaysAll": _ = ParsingCalendarUtf8.ParseOffDaysAll(bytes); break;
            case "OffDaysMarket": _ = ParsingCalendarUtf8.ParseOffDaysMarket(bytes); break;
            case "StockSession": _ = ParsingCalendarUtf8.ParseStockSession(bytes); break;
            case "FuturesSession": _ = ParsingCalendarUtf8.ParseFuturesSession(bytes); break;
            case "FuturesSecurities": _ = ParsingCalendarUtf8.ParseFuturesSecurities(bytes); break;
            case "SuspendedWithReasons": _ = ParsingCalendarUtf8.ParseSuspendedWithReasons(bytes); break;
            case "SecurityChangesWithAttributes": _ = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(bytes); break;
            default: throw new ArgumentException($"Unknown parser: {parser}");
        }
    }

    private static void InvokeIssParser(string parser, byte[] bytes)
    {
        switch (parser)
        {
            case "Stock": _ = ParsingIssUtf8.ParseIssSecurityStock(bytes); break;
            case "Futures": _ = ParsingIssUtf8.ParseIssSecurityFutures(bytes); break;
            default: throw new ArgumentException($"Unknown parser: {parser}");
        }
    }
}
