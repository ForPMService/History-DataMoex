using History_DataMoex.Clients;
using History_DataMoex.Options;
using Microsoft.Extensions.Options;
using System.Net;
using Microsoft.Extensions.Http.Resilience;
namespace History_DataMoex.Infrastructure.DependencyInjection;

public static class MoexClientServiceCollectionExtensions
{
    public static IServiceCollection AddMoexClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MoexIssOptions>()
            .Bind(configuration.GetSection("MoexIss"))
            .Validate(HasValidBaseUrl, "MoexIss:BaseUrl must be a valid absolute URI.")
            .ValidateOnStart();

        services.AddOptions<MoexAlgOptions>()
            .Bind(configuration.GetSection("MoexAlg"))
            .Validate(HasValidBaseUrl, "MoexAlg:BaseUrl must be a valid absolute URI.")
            .ValidateOnStart();

        services.AddHttpClient<MoexHttpIssClient>((sp, client) =>
        {
            MoexIssOptions options = sp.GetRequiredService<IOptions<MoexIssOptions>>().Value;
            ApplyCommonHttpClientOptions(client, options);
        }).ConfigurePrimaryHttpMessageHandler(sp =>
        {
            MoexIssOptions options = sp.GetRequiredService<IOptions<MoexIssOptions>>().Value;
            return CreateDefaultHandler(options);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(5);

            // Учитываем Retry-After из ответа сервера при 429 (rate limit).
            // Если заголовок присутствует — используем его значение вместо
            // дефолтного exponential backoff, чтобы не бомбардировать MOEX.
            options.Retry.DelayGenerator = args =>
            {
                // Polly ретраит 429 на уровне pipeline ДО нашего classifier.
                // Берём Retry-After прямо из HttpResponseMessage.
                if (args.Outcome.Result is HttpResponseMessage
                    { StatusCode: (HttpStatusCode)429 } resp)
                {
                    TimeSpan? delay = null;

                    if (resp.Headers.RetryAfter?.Delta is { } delta)
                        delay = delta;
                    else if (resp.Headers.RetryAfter?.Date is { } date)
                    {
                        var diff = date - DateTimeOffset.UtcNow;
                        delay = diff > TimeSpan.Zero ? diff : TimeSpan.FromSeconds(1);
                    }

                    // Защита: не зависать дольше 2 минут на серверном Retry-After
                    if (delay is not null)
                    {
                        var maxDelay = TimeSpan.FromMinutes(2);
                        if (delay > maxDelay)
                            delay = maxDelay;
                        return ValueTask.FromResult<TimeSpan?>(delay);
                    }
                }
                // Для остальных ошибок — дефолтный exponential backoff Polly
                return ValueTask.FromResult<TimeSpan?>(null);
            };
        }); 

        services.AddHttpClient<MoexHttpAlgClient>((sp, client) =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            ApplyCommonHttpClientOptions(client, options);
        }).ConfigurePrimaryHttpMessageHandler(sp =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            return CreateDefaultHandler(options);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(5);

            // Учитываем Retry-After из ответа сервера при 429 (rate limit).
            // Если заголовок присутствует — используем его значение вместо
            // дефолтного exponential backoff, чтобы не бомбардировать MOEX.
            options.Retry.DelayGenerator = args =>
            {
                // Polly ретраит 429 на уровне pipeline ДО нашего classifier.
                // Берём Retry-After прямо из HttpResponseMessage.
                if (args.Outcome.Result is HttpResponseMessage
                    { StatusCode: (HttpStatusCode)429 } resp)
                {
                    TimeSpan? delay = null;

                    if (resp.Headers.RetryAfter?.Delta is { } delta)
                        delay = delta;
                    else if (resp.Headers.RetryAfter?.Date is { } date)
                    {
                        var diff = date - DateTimeOffset.UtcNow;
                        delay = diff > TimeSpan.Zero ? diff : TimeSpan.FromSeconds(1);
                    }

                    // Защита: не зависать дольше 2 минут на серверном Retry-After
                    if (delay is not null)
                    {
                        var maxDelay = TimeSpan.FromMinutes(2);
                        if (delay > maxDelay)
                            delay = maxDelay;
                        return ValueTask.FromResult<TimeSpan?>(delay);
                    }
                }
                // Для остальных ошибок — дефолтный exponential backoff Polly
                return ValueTask.FromResult<TimeSpan?>(null);
            };
        }); 

        services.AddHttpClient<MoexHttpCalendarClient>((sp, client) =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            ApplyCommonHttpClientOptions(client, options);
        }).ConfigurePrimaryHttpMessageHandler(sp =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            return CreateDefaultHandler(options);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(5);

            // Учитываем Retry-After из ответа сервера при 429 (rate limit).
            // Если заголовок присутствует — используем его значение вместо
            // дефолтного exponential backoff, чтобы не бомбардировать MOEX.
            options.Retry.DelayGenerator = args =>
            {
                // Polly ретраит 429 на уровне pipeline ДО нашего classifier.
                // Берём Retry-After прямо из HttpResponseMessage.
                if (args.Outcome.Result is HttpResponseMessage
                    { StatusCode: (HttpStatusCode)429 } resp)
                {
                    TimeSpan? delay = null;

                    if (resp.Headers.RetryAfter?.Delta is { } delta)
                        delay = delta;
                    else if (resp.Headers.RetryAfter?.Date is { } date)
                    {
                        var diff = date - DateTimeOffset.UtcNow;
                        delay = diff > TimeSpan.Zero ? diff : TimeSpan.FromSeconds(1);
                    }

                    // Защита: не зависать дольше 2 минут на серверном Retry-After
                    if (delay is not null)
                    {
                        var maxDelay = TimeSpan.FromMinutes(2);
                        if (delay > maxDelay)
                            delay = maxDelay;
                        return ValueTask.FromResult<TimeSpan?>(delay);
                    }
                }
                // Для остальных ошибок — дефолтный exponential backoff Polly
                return ValueTask.FromResult<TimeSpan?>(null);
            };
        }); 

        return services;
    }

    private static void ApplyCommonHttpClientOptions(
        HttpClient client,
        MoexClientOptions options)
    {
        client.Timeout = options.RequestTimeout;

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        }
    }
    private static SocketsHttpHandler CreateDefaultHandler(MoexClientOptions options) => new()
    {
        AutomaticDecompression = DecompressionMethods.All,
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        MaxConnectionsPerServer = options.MaxConnectionsPerServer,
    };
    private static bool HasValidBaseUrl(MoexClientOptions options)
    {
        return Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}