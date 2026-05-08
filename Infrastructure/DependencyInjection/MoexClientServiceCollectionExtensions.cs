using History_DataMoex.Clients;
using History_DataMoex.Options;
using Microsoft.Extensions.Options;

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
        });

        services.AddHttpClient<MoexHttpAlgClient>((sp, client) =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            ApplyCommonHttpClientOptions(client, options);
        });

        services.AddHttpClient<MoexHttpCalendarClient>((sp, client) =>
        {
            MoexAlgOptions options = sp.GetRequiredService<IOptions<MoexAlgOptions>>().Value;
            ApplyCommonHttpClientOptions(client, options);
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

    private static bool HasValidBaseUrl(MoexClientOptions options)
    {
        return Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}