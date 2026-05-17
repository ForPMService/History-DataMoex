using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;
using History_DataMoex.Mappers;
using History_DataMoex.Options;
using History_DataMoex.RawStore.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace History_DataMoex.RawStore;

// Manifest фиксирует метаданные первого сохранения raw content.
// При повторном получении идентичного содержимого (AlreadyExisted = true)
// manifest не перезаписывается. Вызывающий код отвечает за фиксацию
// связи LoadJobId → Sha256 на своём уровне (лог, PostgreSQL raw_objects).
public sealed class LocalFileRawObjectStore : IRawObjectStore
{
    private readonly string _root;
    private readonly ILogger<LocalFileRawObjectStore> _logger;
    private readonly IRawStoreFileSystem _fs;

    /// <summary>Public конструктор для DI.</summary>
    public LocalFileRawObjectStore(
        IOptions<RawStoreOptions> options,
        ILogger<LocalFileRawObjectStore> logger)
        : this(options, logger, PhysicalRawStoreFileSystem.Instance)
    {
    }

    /// <summary>Internal конструктор для тестов (FS seam).</summary>
    internal LocalFileRawObjectStore(
        IOptions<RawStoreOptions> options,
        ILogger<LocalFileRawObjectStore> logger,
        IRawStoreFileSystem fs)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Root))
            throw new RawStoreConfigException("RawStore:Root is required.");

        _root = Path.GetFullPath(options.Value.Root);
        _logger = logger;
        _fs = fs;
    }

    public async Task<RawObjectMeta> SaveAsync(
        ReadOnlyMemory<byte> content,
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        CancellationToken ct = default)
    {
        RawStoreLogMessages.RawSaveStarted(
            _logger, context.SourceCode, context.Endpoint, secId, fromDate, tillDate, content.Length);

        byte[] hash = SHA256.HashData(content.Span);
        string sha256Hex = Convert.ToHexStringLower(hash);

        string endpointNormalized = NormalizeEndpoint(context.Endpoint);
        string safeSource = SanitizePathSegment(context.SourceCode);
        string safeEndpoint = SanitizePathSegment(endpointNormalized);
        string safeSecId = SanitizePathSegment(secId);
        string safeDateRange = SanitizePathSegment($"{fromDate}_{tillDate}");

        string rawFileName = sha256Hex + ".json";
        string manifestFileName = sha256Hex + ".manifest.json";

        string storagePath = string.Join('/', safeSource, safeEndpoint, safeSecId, safeDateRange, rawFileName);
        string manifestPath = string.Join('/', safeSource, safeEndpoint, safeSecId, safeDateRange, manifestFileName);

        // fullDir строится строго из safe-сегментов (не из raw vars).
        string fullDir = Path.Combine(_root, safeSource, safeEndpoint, safeSecId, safeDateRange);
        _fs.CreateDirectory(fullDir);

        string rawFilePath = Path.Combine(fullDir, rawFileName);
        string manifestFilePath = Path.Combine(fullDir, manifestFileName);

        // Pre-check: проверяем ОБА файла. AlreadyExisted: true только если есть оба.
        bool rawExists = _fs.FileExists(rawFilePath);
        bool manifestExists = _fs.FileExists(manifestFilePath);

        if (rawExists && manifestExists)
        {
            RawStoreLogMessages.RawSaveSkipped(_logger, context.SourceCode, secId, sha256Hex);
            return new RawObjectMeta(
                context.RawObjectId, storagePath, manifestPath, sha256Hex, content.Length,
                AlreadyExisted: true);
        }

        // Уникальный per-call temp suffix. Без этого параллельные save с одинаковым
        // content столкнутся на одном tempRawPath.
        string tempSuffix = Guid.CreateVersion7().ToString("N");
        string tempRawPath = Path.Combine(fullDir, $"{sha256Hex}.{tempSuffix}.tmp");
        string tempManifestPath = Path.Combine(fullDir, $"{sha256Hex}.{tempSuffix}.manifest.tmp");

        // Recovery: raw уже есть с предыдущего запуска, manifest потерян (kill -9).
        if (rawExists && !manifestExists)
        {
            await WriteManifestWithRecoveryAsync(
                tempManifestPath, manifestFilePath, context, secId, fromDate, tillDate,
                sha256Hex, content.Length, ct).ConfigureAwait(false);

            RawStoreLogMessages.RawSaveSkipped(_logger, context.SourceCode, secId, sha256Hex);
            return new RawObjectMeta(
                context.RawObjectId, storagePath, manifestPath, sha256Hex, content.Length,
                AlreadyExisted: true);
        }

        // ── Нормальный путь: ничего не существует, пишем оба файла ──

        // Write raw temp
        try
        {
            await _fs.WriteAllBytesAsync(tempRawPath, content, ct).ConfigureAwait(false);
        }
        catch (IOException ioEx)
        {
            _fs.DeleteIfExists(tempRawPath);
            var ex = new RawStoreIOException(rawFilePath, ioEx) { Sha256Hex = sha256Hex };
            RawStoreLogMessages.RawSaveFailed(
                _logger, ex, context.SourceCode, secId, ex.ErrorCategory, ex.Message);
            throw ex;
        }

        // Move raw to final
        try
        {
            _fs.Move(tempRawPath, rawFilePath);
        }
        catch (IOException) when (_fs.FileExists(rawFilePath))
        {
            // Race condition: параллельный поток создал target. Дописываем свой manifest.
            _fs.DeleteIfExists(tempRawPath);
            await WriteManifestWithRecoveryAsync(
                tempManifestPath, manifestFilePath, context, secId, fromDate, tillDate,
                sha256Hex, content.Length, ct).ConfigureAwait(false);
            RawStoreLogMessages.RawSaveSkipped(_logger, context.SourceCode, secId, sha256Hex);
            return new RawObjectMeta(
                context.RawObjectId, storagePath, manifestPath, sha256Hex, content.Length,
                AlreadyExisted: true);
        }
        catch (IOException ioEx)
        {
            // Не race — target не существует, настоящая IO-ошибка.
            _fs.DeleteIfExists(tempRawPath);
            var ex = new RawStoreIOException(rawFilePath, ioEx) { Sha256Hex = sha256Hex };
            RawStoreLogMessages.RawSaveFailed(
                _logger, ex, context.SourceCode, secId, ex.ErrorCategory, ex.Message);
            throw ex;
        }

        // Write + move manifest
        try
        {
            ReadOnlyMemory<byte> manifestBytes = BuildManifest(
                context, secId, fromDate, tillDate, sha256Hex, content.Length);
            await _fs.WriteAllBytesAsync(tempManifestPath, manifestBytes, ct).ConfigureAwait(false);
            _fs.Move(tempManifestPath, manifestFilePath);
        }
        catch (IOException) when (_fs.FileExists(manifestFilePath))
        {
            // Race: manifest создал параллельный поток. OK.
            _fs.DeleteIfExists(tempManifestPath);
        }
        catch (IOException ioEx)
        {
            // Raw уже на диске, manifest не написать — аномалия. Упасть громко.
            _fs.DeleteIfExists(tempManifestPath);
            var ex = new RawStoreIOException(manifestFilePath, ioEx)
            {
                Sha256Hex = sha256Hex,
                StoragePath = manifestFilePath,
            };
            RawStoreLogMessages.RawSaveFailed(
                _logger, ex, context.SourceCode, secId, ex.ErrorCategory, ex.Message);
            throw ex;
        }

        RawStoreLogMessages.RawSaveCompleted(
            _logger, context.SourceCode, secId, sha256Hex, storagePath, content.Length);
        return new RawObjectMeta(
            context.RawObjectId, storagePath, manifestPath, sha256Hex, content.Length,
            AlreadyExisted: false);
    }

    /// <summary>
    /// Пишет manifest когда raw уже на диске. Два сценария вызова:
    /// (a) recovery: raw существует с предыдущего прогона, manifest потерян;
    /// (b) race: параллельный поток создал raw, мы пытаемся дописать manifest от себя.
    /// </summary>
    private async Task WriteManifestWithRecoveryAsync(
        string tempManifestPath,
        string manifestFilePath,
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        string sha256Hex,
        long contentLength,
        CancellationToken ct)
    {
        try
        {
            ReadOnlyMemory<byte> manifestBytes = BuildManifest(
                context, secId, fromDate, tillDate, sha256Hex, contentLength);
            await _fs.WriteAllBytesAsync(tempManifestPath, manifestBytes, ct).ConfigureAwait(false);
            _fs.Move(tempManifestPath, manifestFilePath);
        }
        catch (IOException) when (_fs.FileExists(manifestFilePath))
        {
            _fs.DeleteIfExists(tempManifestPath);
        }
        catch (IOException ioEx)
        {
            _fs.DeleteIfExists(tempManifestPath);
            var ex = new RawStoreIOException(manifestFilePath, ioEx)
            {
                Sha256Hex = sha256Hex,
                StoragePath = manifestFilePath,
            };
            RawStoreLogMessages.RawSaveFailed(
                _logger, ex, context.SourceCode, secId, ex.ErrorCategory, ex.Message);
            throw ex;
        }
    }

    /// <summary>
    /// Сериализует манифест через ArrayBufferWriter&lt;byte&gt; + Utf8JsonWriter.
    /// Возвращает ReadOnlyMemory&lt;byte&gt; без копий. MemoryStream + copy-to-array запрещён
    /// (см. grep guard в DoD).
    /// </summary>
    private static ReadOnlyMemory<byte> BuildManifest(
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        string sha256Hex,
        long bytesLength)
    {
        var buffer = new ArrayBufferWriter<byte>();

        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false }))
        {
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
        }

        return buffer.WrittenMemory;
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
            throw new RawStorePathException(value ?? "<null>");

        if (value.Contains("..", StringComparison.Ordinal) ||
            value.Contains('/') ||
            value.Contains('\\') ||
            value.Contains(':'))
        {
            throw new RawStorePathException(value);
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (value.Contains(c))
                throw new RawStorePathException(value);
        }

        return value;
    }
}
