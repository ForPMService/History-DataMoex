using System.IO;

namespace History_DataMoex.RawStore.Errors;

public sealed class RawStoreIOException : RawStoreException
{
    public RawStoreIOException(string storagePath, IOException inner)
        : base($"IO error writing raw object at '{storagePath}'", inner)
    {
        StoragePath = storagePath;
        IsRetryable = true;
    }

    public override string ErrorCategory => "raw_io";
}
