using History_DataMoex.Mappers;
using History_DataMoex.RawStore;

namespace TestHistoryData.Phase8;

/// <summary>
/// Test double для IRawObjectStore. Не пишет на диск.
/// Считает вызовы и сохраняет последний context для assertions.
/// Возвращает RawObjectMeta с context.RawObjectId — точно как LocalFileRawObjectStore.
///
/// RawObjectMeta — positional record (Guid, string, string, string, long, bool),
/// используется constructor call, НЕ object initializer.
/// </summary>
public sealed class FakeRawObjectStore : IRawObjectStore
{
    public int SaveCallCount { get; private set; }
    public MapContext? LastSaveContext { get; private set; }
    public string? LastSecId { get; private set; }
    public string? LastFromDate { get; private set; }
    public string? LastTillDate { get; private set; }
    public int LastContentLength { get; private set; }
    public bool ThrowOnSave { get; set; }

    public Task<RawObjectMeta> SaveAsync(
        ReadOnlyMemory<byte> content,
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        CancellationToken ct = default)
    {
        SaveCallCount++;
        LastSaveContext = context;
        LastSecId = secId;
        LastFromDate = fromDate;
        LastTillDate = tillDate;
        LastContentLength = content.Length;

        if (ThrowOnSave)
            throw new InvalidOperationException("FakeRawObjectStore configured to throw on SaveAsync.");

        // Positional constructor — точно как LocalFileRawObjectStore: RawObjectId из context.
        RawObjectMeta meta = new RawObjectMeta(
            RawObjectId: context.RawObjectId,
            StoragePath: $"fake/{context.SourceCode}/storage.json",
            ManifestPath: $"fake/{context.SourceCode}/storage.manifest.json",
            Sha256Hex: "fake-sha256",
            BytesLength: content.Length,
            AlreadyExisted: false);

        return Task.FromResult(meta);
    }
}

/// <summary>
/// Mock HttpMessageHandler — возвращает заранее заданный response body.
/// Status code 200 OK по умолчанию.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseBody;
    private readonly System.Net.HttpStatusCode _statusCode;

    public FakeHttpMessageHandler(string responseBody, System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json"),
        };
        return Task.FromResult(response);
    }
}
