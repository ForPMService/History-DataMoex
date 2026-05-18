using History_DataMoex.Clients;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using History_DataMoex.Options;
using History_DataMoex.Parsing.Errors;
using History_DataMoex.RawCapture;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace TestHistoryData.Phase8;

public class RawCaptureTests
{
    // Минимальный валидный JSON для OffDaysAll endpoint (single-table, без cursor).
    private const string ValidOffDaysAllJson = """
    {
      "off_days": {
        "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"],
        "data": [
          ["2026-01-01", 0, null, "H", 0, null, "H", 0, null, "H"]
        ]
      }
    }
    """;

    // Минимальный валидный JSON для Suspended+Reasons endpoint (multi-table с cursor).
    private const string ValidSuspendedJson = """
    {
      "suspended": {
        "columns": ["secid", "reason_id", "date_from", "date_till", "boardid", "settle_codes", "changedate", "updatetime"],
        "data": [
          ["SBER", "1", "2026-01-05", null, "TQBR", "Y2", "2025-12-30", "2025-12-30 10:00:00"]
        ]
      },
      "suspended.reasons": {
        "columns": ["id", "title"],
        "data": [
          [1, "Test reason"]
        ]
      },
      "suspended.cursor": {
        "columns": ["INDEX", "TOTAL", "PAGESIZE"],
        "data": [
          [0, 100, 100]
        ]
      }
    }
    """;

    // Структурно битый JSON — без секции "data".
    private const string BrokenJson = """
    {
      "off_days": {
        "columns": ["tradedate", "currency_workday", "currency_trade_session_date", "currency_reason", "futures_workday", "futures_trade_session_date", "futures_reason", "stock_workday", "stock_trade_session_date", "stock_reason"]
      }
    }
    """;

    // Валидный JSON со сломанным DTO — Suspended с пустым secid ("-") → Group A валидация в маппере падает.
    private const string MapValidationFailJson = """
    {
      "suspended": {
        "columns": ["secid", "reason_id", "date_from", "date_till", "boardid", "settle_codes", "changedate", "updatetime"],
        "data": [
          ["-", "1", "2026-01-05", null, "TQBR", "Y2", "2025-12-30", "2025-12-30 10:00:00"]
        ]
      },
      "suspended.reasons": {
        "columns": ["id", "title"],
        "data": []
      },
      "suspended.cursor": {
        "columns": ["INDEX", "TOTAL", "PAGESIZE"],
        "data": [
          [0, 100, 100]
        ]
      }
    }
    """;

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
    public async Task RawCapture_SaveBeforeParse_RawObjectIdPropagated()
    {
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(ValidOffDaysAllJson, store);

        SourcePage<CalendarOffDayAll> result = await client.GetOffDaysAllRawAsync();

        Assert.Equal(1, store.SaveCallCount);
        Assert.NotNull(store.LastSaveContext);
        Assert.NotEqual(Guid.Empty, store.LastSaveContext!.RawObjectId);
        Assert.Equal(store.LastSaveContext.RawObjectId, result.RawObjectId);

        Assert.NotEmpty(result.Items);
        foreach (CalendarOffDayAll m in result.Items)
        {
            Assert.Equal(result.RawObjectId, m.RawObjectId);
        }
    }

    [Fact]
    public async Task RawCapture_ParseFails_RawStillSaved()
    {
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(BrokenJson, store);

        await Assert.ThrowsAsync<MoexSchemaMismatchException>(
            () => client.GetOffDaysAllRawAsync());

        // Save был ДО парсинга (Lock §2) — счётчик увеличился, несмотря на parser exception.
        Assert.Equal(1, store.SaveCallCount);
    }

    [Fact]
    public async Task RawCapture_MapBatchFails_RawStillSaved()
    {
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(MapValidationFailJson, store);

        // Map валидация Group A — SecId="-" → MappingValidationException.
        await Assert.ThrowsAsync<MappingValidationException>(
            () => client.GetSuspendedWithReasonsRawAsync());

        // Save был до парсинга и до маппинга — saved несмотря на validation failure.
        Assert.Equal(1, store.SaveCallCount);
    }

    [Fact]
    public async Task RawCapture_MultiTable_BothItemsShareRawObjectId()
    {
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(ValidSuspendedJson, store);

        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> result =
            await client.GetSuspendedWithReasonsRawAsync();

        Assert.Equal(1, store.SaveCallCount);
        Assert.NotEmpty(result.MainItems);
        Assert.NotEmpty(result.SecondaryItems);

        // Multi-table cohesion (Lock §6): обе таблицы получают один RawObjectId.
        foreach (CalendarSuspension m in result.MainItems)
            Assert.Equal(result.RawObjectId, m.RawObjectId);
        foreach (CalendarSuspensionReason s in result.SecondaryItems)
            Assert.Equal(result.RawObjectId, s.RawObjectId);
    }

    [Fact]
    public async Task RawCapture_FetchedAtUtc_Consistent()
    {
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(ValidSuspendedJson, store);

        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> result =
            await client.GetSuspendedWithReasonsRawAsync();

        Assert.NotEmpty(result.MainItems);
        Assert.NotEmpty(result.SecondaryItems);

        foreach (CalendarSuspension m in result.MainItems)
            Assert.Equal(result.FetchedAtUtc, m.FetchedAtUtc);
        foreach (CalendarSuspensionReason s in result.SecondaryItems)
            Assert.Equal(result.FetchedAtUtc, s.FetchedAtUtc);
    }
}
