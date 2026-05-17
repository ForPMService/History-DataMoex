using Microsoft.Extensions.Logging;

namespace History_DataMoex.RawStore;

public static partial class RawStoreLogMessages
{
    [LoggerMessage(
        EventId = 200,
        EventName = "RawSaveStarted",
        Level = LogLevel.Information,
        Message = "Raw save started: source={SourceCode}, endpoint={Endpoint}, secId={SecId}, from={From}, till={Till}, bytes={BytesLength}.")]
    public static partial void RawSaveStarted(
        ILogger logger,
        string sourceCode,
        string endpoint,
        string secId,
        string from,
        string till,
        long bytesLength);

    [LoggerMessage(
        EventId = 201,
        EventName = "RawSaveCompleted",
        Level = LogLevel.Information,
        Message = "Raw save completed: source={SourceCode}, secId={SecId}, sha256={Sha256}, path={StoragePath}, bytes={BytesLength}.")]
    public static partial void RawSaveCompleted(
        ILogger logger,
        string sourceCode,
        string secId,
        string sha256,
        string storagePath,
        long bytesLength);

    [LoggerMessage(
        EventId = 202,
        EventName = "RawSaveSkipped",
        Level = LogLevel.Information,
        Message = "Raw save skipped (already exists): source={SourceCode}, secId={SecId}, sha256={Sha256}.")]
    public static partial void RawSaveSkipped(
        ILogger logger,
        string sourceCode,
        string secId,
        string sha256);

    [LoggerMessage(
        EventId = 203,
        EventName = "RawSaveFailed",
        Level = LogLevel.Error,
        Message = "Raw save failed: source={SourceCode}, secId={SecId}, errorCategory={ErrorCategory}, message={ErrorMessage}.")]
    public static partial void RawSaveFailed(
        ILogger logger,
        Exception exception,
        string sourceCode,
        string secId,
        string errorCategory,
        string errorMessage);
}
