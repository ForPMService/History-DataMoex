using System.Net;
using System.Net.Http.Headers;
using History_DataMoex.Clients;
using History_DataMoex.Clients.Errors;

namespace TestHistoryData;

public class TypedErrorsTests
{
    // ═══════════════════════════════════════════════════════════
    // 1. 429 → MoexRateLimitException, IsRetryable = true
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns429_ThrowsMoexRateLimitException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

        var ex = Assert.Throws<MoexRateLimitException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.True(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 2. 429 + Retry-After: Delta → парсится в RetryAfter
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns429WithRetryAfterSeconds_ParsesCorrectly()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));

        var ex = Assert.Throws<MoexRateLimitException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.Equal(TimeSpan.FromSeconds(5), ex.RetryAfter);
    }

    // ═══════════════════════════════════════════════════════════
    // 3. 429 без Retry-After → ex.RetryAfter == null
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns429WithoutRetryAfter_RetryAfterIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

        var ex = Assert.Throws<MoexRateLimitException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.Null(ex.RetryAfter);
    }

    // ═══════════════════════════════════════════════════════════
    // 4. 401 → MoexAuthException, IsRetryable = false
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns401_ThrowsMoexAuthException_NotRetryable()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var ex = Assert.Throws<MoexAuthException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.False(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 5. 403 → MoexAuthException, IsRetryable = false
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns403_ThrowsMoexAuthException_NotRetryable()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Forbidden);

        var ex = Assert.Throws<MoexAuthException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.False(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 6. 400 → MoexBadRequestException, IsRetryable = false
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns400_ThrowsMoexBadRequestException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        var ex = Assert.Throws<MoexBadRequestException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.False(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 7. 404 → MoexNotFoundException, IsRetryable = false
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns404_ThrowsMoexNotFoundException()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var ex = Assert.Throws<MoexNotFoundException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.False(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 8. 500 → MoexServerException, IsRetryable = true
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns500_ThrowsMoexServerException_Retryable()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var ex = Assert.Throws<MoexServerException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test/endpoint"));

        Assert.True(ex.IsRetryable);
    }

    // ═══════════════════════════════════════════════════════════
    // 9. 200 → не бросает, response не disposed
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void Returns200_DoesNotThrow()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        HttpClientHelpers.EnsureSuccessOrThrow(response, "test");

        // response не disposed — Content доступен без ObjectDisposedException
        Assert.NotNull(response.Content);
    }

    // ═══════════════════════════════════════════════════════════
    // 10. Ошибочный ответ — response.Dispose() вызывается до throw
    // ═══════════════════════════════════════════════════════════

    [Fact]
    public void EnsureSuccessOrThrow_DisposesResponseOnError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("body")
        };

        Assert.Throws<MoexBadRequestException>(
            () => HttpClientHelpers.EnsureSuccessOrThrow(response, "test"));

        Assert.Throws<ObjectDisposedException>(() =>
        {
            using Stream _ = response.Content.ReadAsStream();
        });
    }
}
