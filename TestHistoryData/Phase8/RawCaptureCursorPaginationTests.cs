using History_DataMoex.Clients;
using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Models;
using History_DataMoex.Options;
using History_DataMoex.RawCapture;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;

namespace TestHistoryData.Phase8;

/// <summary>
/// Cursor pagination integration tests для 8-C raw методов с cursor (Suspended, SecurityChanges).
/// Lock §6 (multi-table cohesion), §15 (raw lineage). Расширяет 5 critical RawCaptureTests.cs.
/// </summary>
public class RawCaptureCursorPaginationTests
{
    private static MoexHttpCalendarClient BuildCalendarClient(
        SequencedFakeHttpMessageHandler handler, FakeRawObjectStore store)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://iss.moex.com/iss") };
        var options = Options.Create(new MoexCalendarOptions
        {
            BaseUrl = "https://iss.moex.com/iss",
            Key = "test-key",
        });
        return new MoexHttpCalendarClient(options, httpClient, store, NullLogger<MoexHttpCalendarClient>.Instance);
    }

    // Minimal Suspended JSON с заданным cursor (минимальное тело — пустые data + cursor).
    private static string BuildSuspendedJson(int? cursorIndex, int? cursorTotal, int? cursorPageSize)
    {
        string indexLit = cursorIndex.HasValue ? cursorIndex.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null";
        string totalLit = cursorTotal.HasValue ? cursorTotal.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null";
        string pageLit = cursorPageSize.HasValue ? cursorPageSize.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null";

        return $$"""
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
            "data": [[{{indexLit}}, {{totalLit}}, {{pageLit}}]]
          }
        }
        """;
    }

    [Fact]
    public async Task CursorPagination_TwoPagesLoop_BothPagesProcessed()
    {
        string page1 = BuildSuspendedJson(0, 200, 100);
        string page2 = BuildSuspendedJson(100, 200, 100);

        var handler = new SequencedFakeHttpMessageHandler(new[] { page1, page2 });
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(handler, store);

        // Page 1 — start=0 (default).
        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> r1 =
            await client.GetSuspendedWithReasonsRawAsync();

        Assert.NotNull(r1.NextCursor);
        Assert.Equal(0, r1.NextCursor!.Index);
        Assert.Equal(200, r1.NextCursor.Total);

        // Caller использует MoexCursorPagination.Next для следующего шага.
        PaginationStep step1 = MoexCursorPagination.Next(r1.NextCursor, pagesElapsed: 1, maxPagesGuard: 100);
        Assert.False(step1.IsStop);
        Assert.Equal(100, step1.NextStart);

        // Page 2 — start=100.
        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> r2 =
            await client.GetSuspendedWithReasonsRawAsync(start: step1.NextStart);

        PaginationStep step2 = MoexCursorPagination.Next(r2.NextCursor!, pagesElapsed: 2, maxPagesGuard: 100);
        Assert.True(step2.IsStop);
        Assert.Equal("range_exhausted", step2.StopReason);

        Assert.Equal(2, store.SaveCallCount);
        Assert.Equal(2, handler.CallCount);
        // Каждый вызов получает свежий RawObjectId.
        Assert.NotEqual(r1.RawObjectId, r2.RawObjectId);
    }

    [Fact]
    public async Task CursorPagination_EmptyCursor_StopsAfterFirstPage()
    {
        // Cursor.Index=null означает empty_cursor (MoexCursorPagination.Next).
        string json = BuildSuspendedJson(null, null, null);
        var handler = new SequencedFakeHttpMessageHandler(new[] { json });
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(handler, store);

        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> r =
            await client.GetSuspendedWithReasonsRawAsync();

        // NextCursor null когда все поля курсора null.
        Assert.Null(r.NextCursor);
        Assert.Equal(1, store.SaveCallCount);

        // Caller вычисляет шаг — должен получить empty_cursor.
        PaginationStep step = MoexCursorPagination.Next(
            r.NextCursor ?? new PaginationCursorDTO(),
            pagesElapsed: 1, maxPagesGuard: 100);
        Assert.True(step.IsStop);
        Assert.Equal("empty_cursor", step.StopReason);
    }

    [Fact]
    public async Task CursorPagination_RangeExhausted_StopsAtBoundary()
    {
        // Index + PageSize == Total → range_exhausted.
        string json = BuildSuspendedJson(900, 1000, 100);
        var handler = new SequencedFakeHttpMessageHandler(new[] { json });
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(handler, store);

        SourceMultiTable<CalendarSuspension, CalendarSuspensionReason> r =
            await client.GetSuspendedWithReasonsRawAsync();

        Assert.NotNull(r.NextCursor);
        PaginationStep step = MoexCursorPagination.Next(r.NextCursor!, pagesElapsed: 1, maxPagesGuard: 100);

        Assert.True(step.IsStop);
        Assert.Equal("range_exhausted", step.StopReason);
        Assert.Equal(1, store.SaveCallCount);
    }

    [Fact]
    public async Task CursorPagination_SafetyCap_HitMaxPagesGuard()
    {
        // Cursor возвращает Index=0, Total=10000, PageSize=100 — бесконечно много страниц.
        // Caller с maxPagesGuard=2 должен остановиться через safety_cap_hit.
        string body = BuildSuspendedJson(0, 10000, 100);
        var handler = new SequencedFakeHttpMessageHandler(new[] { body, body });
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(handler, store);

        // Page 1.
        var r1 = await client.GetSuspendedWithReasonsRawAsync();
        PaginationStep s1 = MoexCursorPagination.Next(r1.NextCursor!, pagesElapsed: 1, maxPagesGuard: 2);
        Assert.False(s1.IsStop);

        // Page 2 — pagesElapsed становится 2, == maxPagesGuard → safety_cap_hit.
        var r2 = await client.GetSuspendedWithReasonsRawAsync(start: s1.NextStart);
        PaginationStep s2 = MoexCursorPagination.Next(r2.NextCursor!, pagesElapsed: 2, maxPagesGuard: 2);
        Assert.True(s2.IsStop);
        Assert.Equal("safety_cap_hit", s2.StopReason);
        Assert.Equal(2, store.SaveCallCount);
    }

    [Fact]
    public async Task CursorPagination_StartParameter_PassedToHttpQueryParams()
    {
        string json = BuildSuspendedJson(500, 1000, 100);
        var handler = new SequencedFakeHttpMessageHandler(new[] { json });
        var store = new FakeRawObjectStore();
        MoexHttpCalendarClient client = BuildCalendarClient(handler, store);

        _ = await client.GetSuspendedWithReasonsRawAsync(start: 500);

        Assert.NotNull(handler.LastRequestUri);
        string url = handler.LastRequestUri!.ToString();
        Assert.Contains("start=500", url);
    }

    /// <summary>
    /// Mock HttpMessageHandler с заранее заданной последовательностью ответов.
    /// Возвращает responses[CallCount-1] на каждый запрос. Записывает LastRequestUri.
    /// Не модифицирует общий <see cref="FakeHttpMessageHandler"/> из Phase8RawCaptureTestsHelpers.cs.
    /// </summary>
    private sealed class SequencedFakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string[] _responses;
        public int CallCount { get; private set; }
        public Uri? LastRequestUri { get; private set; }

        public SequencedFakeHttpMessageHandler(string[] responses)
        {
            _responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            int idx = Math.Min(CallCount, _responses.Length - 1);
            CallCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responses[idx], System.Text.Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
