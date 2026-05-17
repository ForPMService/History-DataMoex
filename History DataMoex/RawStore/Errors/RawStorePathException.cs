namespace History_DataMoex.RawStore.Errors;

public sealed class RawStorePathException : RawStoreException
{
    public string Segment { get; }

    public RawStorePathException(string segment)
        : base($"Invalid path segment for raw store: '{segment}'")
    {
        Segment = segment;
        IsRetryable = false;
    }

    public override string ErrorCategory => "raw_path";
}
