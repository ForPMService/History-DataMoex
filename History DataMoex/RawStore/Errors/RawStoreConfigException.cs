namespace History_DataMoex.RawStore.Errors;

public sealed class RawStoreConfigException : RawStoreException
{
    public RawStoreConfigException(string message) : base(message)
    {
        IsRetryable = false;
    }

    public override string ErrorCategory => "raw_config";
}
