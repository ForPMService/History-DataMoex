using History_DataMoex.Clients;
using History_DataMoex.Models;
using History_DataMoex.Options;
using History_DataMoex.RawCapture;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace TestHistoryData.Phase8;

/// <summary>
/// Multi-table integration с реальными парсерами (не mock). Расширение 5 critical RawCaptureTests.cs.
/// Lock §6: multi-table cohesion (один RawObjectId на обе таблицы из одного HTTP-ответа).
/// </summary>
public class RawCaptureMultiTableIntegrationTests
{
    private static MoexHttpCalendarClient BuildCalendarClient(string responseJson, FakeRawObjectStore store)
    {
        var handler = new FakeHttpMessageHandler(responseJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://iss.moex.com/iss") };
        var options = Options.Create(new MoexCalendarOptions
        {
            BaseUrl = "https://iss.moex.com/iss",
            Key = "test-key",
        });
        return new MoexHttpCalendarClient(options, httpClient, store, NullLogger<MoexHttpCalendarClient>.Instance);
    }

    [Fact]
    public async Task FuturesSecurities_RealParser_FortsAndOptionsSeparateLists()
    {
        const string json = """
        {
          "forts": {
            "columns": ["secid", "asset_code", "shortname", "exec_type", "contract_name", "expiration_date", "end_date", "expiration_type", "expiration_time", "weekend_session"],
            "data": [
              ["SiM6", "Si", "Si-6.26", "FX", "USD/RUB June 2026", "2026-06-18", "2026-06-18", "Standard", "18:45:00", 0],
              ["BRN6", "BR", "BR-6.26", "FX", "Brent June 2026", "2026-06-30", "2026-06-30", "Standard", "18:45:00", 0]
            ]
          },
          "options": {
            "columns": ["asset_type_name", "asset_code", "series_name", "series_type", "exec_type", "margin_style", "contract_name", "expiration_date", "expiration_type", "expiration_time", "weekend_session"],
            "data": [
              ["Currency", "Si", "Si-6.26M", "Standard", "FX", "Future", "USD/RUB Opt June 2026", "2026-06-18", "Standard", "18:45:00", 0]
            ]
          }
        }
        """;
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(json, store);

        SourceMultiTable<CalendarFortsContract, CalendarOptionsSeries> result =
            await client.GetFuturesSecuritiesAllRawAsync();

        Assert.Equal(2, result.MainItems.Count);
        Assert.Single(result.SecondaryItems);
        Assert.Null(result.NextCursor);

        foreach (CalendarFortsContract f in result.MainItems)
            Assert.Equal(result.RawObjectId, f.RawObjectId);
        foreach (CalendarOptionsSeries o in result.SecondaryItems)
            Assert.Equal(result.RawObjectId, o.RawObjectId);
    }

    [Fact]
    public async Task StockSession_RealParser_SessionsAndTypesSeparate()
    {
        const string json = """
        {
          "session_schedule": {
            "columns": ["tradedate", "tradingsession", "boardid", "secid", "type", "time_from", "time_till", "updatetime"],
            "data": [
              ["2026-05-17", 1, "TQBR", "-", "PREOPEN", "09:50:00", "10:00:00", "2026-05-17 09:00:00"],
              ["2026-05-17", 2, "TQBR", "-", "MAIN", "10:00:00", "18:45:00", "2026-05-17 09:00:00"]
            ]
          },
          "session_schedule.types": {
            "columns": ["type", "title"],
            "data": [
              ["PREOPEN", "Аукцион открытия"],
              ["MAIN", "Основная сессия"]
            ]
          }
        }
        """;
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(json, store);

        SourceMultiTable<CalendarStockSession, CalendarSessionType> result =
            await client.GetStockSessionWithTypesRawAsync();

        Assert.Equal(2, result.MainItems.Count);
        Assert.Equal(2, result.SecondaryItems.Count);
        Assert.Null(result.NextCursor);

        foreach (CalendarStockSession s in result.MainItems)
            Assert.Equal(result.RawObjectId, s.RawObjectId);
        foreach (CalendarSessionType t in result.SecondaryItems)
        {
            Assert.Equal(result.RawObjectId, t.RawObjectId);
            Assert.Equal("stock", t.Market);
        }
    }

    [Fact]
    public async Task FuturesSession_RealParser_SessionsAndTypesSeparate()
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
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(json, store);

        SourceMultiTable<CalendarFuturesSession, CalendarSessionType> result =
            await client.GetFuturesSessionWithTypesRawAsync();

        Assert.Single(result.MainItems);
        Assert.Single(result.SecondaryItems);
        Assert.Null(result.NextCursor);
        Assert.Equal("futures", result.SecondaryItems[0].Market);

        foreach (CalendarFuturesSession s in result.MainItems)
            Assert.Equal(result.RawObjectId, s.RawObjectId);
        foreach (CalendarSessionType t in result.SecondaryItems)
            Assert.Equal(result.RawObjectId, t.RawObjectId);
    }

    [Fact]
    public async Task SecurityChanges_RealParser_NextCursorPropagated()
    {
        const string json = """
        {
          "securities": {
            "columns": ["updatetime", "action", "secid", "attribute_name", "before_value", "after_value"],
            "data": [
              ["2026-05-07 00:21:04", "updated", "SBER", "LOTSIZE", "10", "1"]
            ]
          },
          "securities.attributes": {
            "columns": ["name", "type", "title"],
            "data": [
              ["LOTSIZE", "I", "Размер лота"]
            ]
          },
          "securities.cursor": {
            "columns": ["INDEX", "TOTAL", "PAGESIZE"],
            "data": [[0, 500, 100]]
          }
        }
        """;
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(json, store);

        SourceMultiTable<CalendarSecurityChange, CalendarSecurityAttribute> result =
            await client.GetSecurityChangesWithAttributesRawAsync();

        Assert.NotNull(result.NextCursor);
        Assert.Equal(0, result.NextCursor!.Index);
        Assert.Equal(500, result.NextCursor.Total);
        Assert.Equal(100, result.NextCursor.PageSize);

        foreach (CalendarSecurityChange c in result.MainItems)
            Assert.Equal(result.RawObjectId, c.RawObjectId);
        foreach (CalendarSecurityAttribute a in result.SecondaryItems)
            Assert.Equal(result.RawObjectId, a.RawObjectId);
    }

    [Fact]
    public async Task OffDaysAll_NoCursor_NextCursorNull()
    {
        const string json = """
        {
          "off_days": {
            "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"],
            "data": [
              ["2026-01-01", 0, null, "H", 0, null, "H", 0, null, "H"]
            ]
          }
        }
        """;
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(json, store);

        SourcePage<CalendarOffDayAll> result = await client.GetOffDaysAllRawAsync();

        Assert.Single(result.Items);
        Assert.Null(result.NextCursor);
        foreach (CalendarOffDayAll m in result.Items)
            Assert.Equal(result.RawObjectId, m.RawObjectId);
    }
}
