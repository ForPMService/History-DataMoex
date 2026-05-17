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
        // ── Timeout budget ──
        // TotalRequestTimeout = 10 мин (весь запрос включая все retry).
        // AttemptTimeout = 2 мин (одна попытка).
        // maxDelay Retry-After = 2 мин (ожидание перед retry при 429).
        // Худший случай одного retry-цикла: 2 мин (wait) + 2 мин (attempt) = 4 мин.
        // При TotalRequestTimeout = 10 мин реально возможны 2–3 retry при длинном Retry-After,
        // а не 5 (дефолт MaxRetryAttempts). Это осознанное поведение:
        // лучше отдать управление вызывающему коду, чем зависнуть на 20 минут.
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
                if (args.Outcome.Result is HttpResponseMessage { StatusCode: (HttpStatusCode)429 } resp)
                {
                    var delay = HttpClientHelpers.GetRetryAfterForPolly(resp, TimeSpan.FromMinutes(2));
                    if (delay is not null)
                        return ValueTask.FromResult<TimeSpan?>(delay);
                }
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
                if (args.Outcome.Result is HttpResponseMessage { StatusCode: (HttpStatusCode)429 } resp)
                {
                    var delay = HttpClientHelpers.GetRetryAfterForPolly(resp, TimeSpan.FromMinutes(2));
                    if (delay is not null)
                        return ValueTask.FromResult<TimeSpan?>(delay);
                }
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
                if (args.Outcome.Result is HttpResponseMessage { StatusCode: (HttpStatusCode)429 } resp)
                {
                    var delay = HttpClientHelpers.GetRetryAfterForPolly(resp, TimeSpan.FromMinutes(2));
                    if (delay is not null)
                        return ValueTask.FromResult<TimeSpan?>(delay);
                }
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