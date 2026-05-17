using System.Security.Cryptography;
using System.Text.Json;
using History_DataMoex.Mappers;
using History_DataMoex.Options;
using Microsoft.Extensions.Options;

namespace History_DataMoex.RawStore;

// Manifest фиксирует метаданные первого сохранения raw content.
// При повторном получении идентичного содержимого (AlreadyExisted = true)
// manifest не перезаписывается. Вызывающий код отвечает за фиксацию
// связи LoadJobId → Sha256 на своём уровне (лог, PostgreSQL raw_objects).
public sealed class LocalFileRawObjectStore : IRawObjectStore
{
    private readonly string _root;

    public LocalFileRawObjectStore(IOptions<RawStoreOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Root))
            throw new ArgumentException("RawStore:Root is required.");

        _root = Path.GetFullPath(options.Value.Root);
    }

    public async Task<RawObjectMeta> SaveAsync(
        ReadOnlyMemory<byte> content,
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        CancellationToken ct = default)
    {
        byte[] hash = SHA256.HashData(content.Span);
        string sha256Hex = Convert.ToHexStringLower(hash);

        string endpointNormalized = NormalizeEndpoint(context.Endpoint);

        string safeSource = SanitizePathSegment(context.SourceCode);
        string safeEndpoint = SanitizePathSegment(endpointNormalized);
        string safeSecId = SanitizePathSegment(secId);
        string safeDateRange = SanitizePathSegment($"{fromDate}_{tillDate}");

        string rawFileName = sha256Hex + ".json";
        string manifestFileName = sha256Hex + ".manifest.json";

        string storagePath = string.Join('/',
            safeSource,
            safeEndpoint,
            safeSecId,
            safeDateRange,
            rawFileName);

        string manifestPath = string.Join('/',
            safeSource,
            safeEndpoint,
            safeSecId,
            safeDateRange,
            manifestFileName);

        string fullDir = Path.Combine(_root,
            context.SourceCode,
            endpointNormalized,
            secId,
            $"{fromDate}_{tillDate}");

        Directory.CreateDirectory(fullDir);

        string rawFilePath = Path.Combine(fullDir, rawFileName);
        string manifestFilePath = Path.Combine(fullDir, manifestFileName);

        if (File.Exists(rawFilePath))
        {
            return new RawObjectMeta(
                RawObjectId: context.RawObjectId,
                StoragePath: storagePath,
                ManifestPath: manifestPath,
                Sha256Hex: sha256Hex,
                BytesLength: content.Length,
                AlreadyExisted: true);
        }

        string tempRawPath = Path.Combine(fullDir, sha256Hex + ".tmp");
        string tempManifestPath = Path.Combine(fullDir, sha256Hex + ".manifest.tmp");

        await File.WriteAllBytesAsync(tempRawPath, content.ToArray(), ct).ConfigureAwait(false);

        try
        {
            File.Move(tempRawPath, rawFilePath);
        }
        catch (IOException)
        {
            try { File.Delete(tempRawPath); } catch { /* best-effort */ }
            return new RawObjectMeta(
                RawObjectId: context.RawObjectId,
                StoragePath: storagePath,
                ManifestPath: manifestPath,
                Sha256Hex: sha256Hex,
                BytesLength: content.Length,
                AlreadyExisted: true);
        }

        byte[] manifestBytes = BuildManifest(context, secId, fromDate, tillDate, sha256Hex, content.Length);
        await File.WriteAllBytesAsync(tempManifestPath, manifestBytes, ct).ConfigureAwait(false);
        File.Move(tempManifestPath, manifestFilePath);

        return new RawObjectMeta(
            RawObjectId: context.RawObjectId,
            StoragePath: storagePath,
            ManifestPath: manifestPath,
            Sha256Hex: sha256Hex,
            BytesLength: content.Length,
            AlreadyExisted: false);
    }

    private static byte[] BuildManifest(
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        string sha256Hex,
        long bytesLength)
    {
        using var ms = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WriteString("raw_object_id", context.RawObjectId.ToString("D"));
        writer.WriteString("load_job_id", context.LoadJobId.ToString("D"));
        writer.WriteString("source_code", context.SourceCode);
        writer.WriteString("endpoint", context.Endpoint);
        writer.WriteString("sec_id", secId);
        writer.WriteString("from", fromDate);
        writer.WriteString("till", tillDate);
        writer.WriteString("fetched_at_utc", context.FetchedAtUtc.ToString("O"));
        writer.WriteString("sha256", sha256Hex);
        writer.WriteNumber("bytes_length", bytesLength);
        writer.WriteEndObject();
        writer.Flush();

        return ms.ToArray();
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        string result = endpoint.TrimStart('/');
        if (result.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            result = result[..^5];
        result = result.Replace('/', '-');
        return result;
    }

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Path segment is empty.");

        if (value.Contains("..", StringComparison.Ordinal) ||
            value.Contains('/') ||
            value.Contains('\\') ||
            value.Contains(':'))
            throw new ArgumentException($"Invalid path segment: {value}");

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (value.Contains(c))
                throw new ArgumentException($"Invalid path segment: {value}");
        }

        return value;
    }
}
