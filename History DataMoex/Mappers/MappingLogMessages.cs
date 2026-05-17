using Microsoft.Extensions.Logging;

namespace History_DataMoex.Mappers;

public static partial class MappingLogMessages
{
    [LoggerMessage(
        EventId = 210,
        EventName = "MapBatchStarted",
        Level = LogLevel.Information,
        Message = "Map batch started: source={SourceCode}, secId={SecId}, category={Category}, dtoCount={DtoCount}.")]
    public static partial void MapBatchStarted(
        ILogger logger,
        string sourceCode,
        string secId,
        string category,
        int dtoCount);

    [LoggerMessage(
        EventId = 211,
        EventName = "MapBatchCompleted",
        Level = LogLevel.Information,
        Message = "Map batch completed: source={SourceCode}, secId={SecId}, category={Category}, mappedCount={MappedCount}, time={ElapsedMs}.")]
    public static partial void MapBatchCompleted(
        ILogger logger,
        string sourceCode,
        string secId,
        string category,
        int mappedCount,
        TimeSpan elapsedMs);

    [LoggerMessage(
        EventId = 212,
        EventName = "MapRowFailed",
        Level = LogLevel.Error,
        Message = "Map row failed: source={SourceCode}, secId={SecId}, category={Category}, rowIndex={RowIndex}, errorCategory={ErrorCategory}, message={ErrorMessage}.")]
    public static partial void MapRowFailed(
        ILogger logger,
        Exception exception,
        string sourceCode,
        string secId,
        string category,
        int rowIndex,
        string errorCategory,
        string errorMessage);

    [LoggerMessage(
        EventId = 213,
        EventName = "MapBatchCancelled",
        Level = LogLevel.Information,
        Message = "Map batch cancelled by caller: source={SourceCode}, secId={SecId}, category={Category}, mappedSoFar={MappedSoFar}.")]
    public static partial void MapBatchCancelled(
        ILogger logger,
        Exception exception,
        string sourceCode,
        string secId,
        string category,
        int mappedSoFar);

    [LoggerMessage(
        EventId = 214,
        EventName = "MapBatchFailed",
        Level = LogLevel.Error,
        Message = "Map batch failed (setup error): source={SourceCode}, secId={SecId}, category={Category}, errorCategory={ErrorCategory}, message={ErrorMessage}.")]
    public static partial void MapBatchFailed(
        ILogger logger,
        Exception exception,
        string sourceCode,
        string secId,
        string category,
        string errorCategory,
        string errorMessage);
}
