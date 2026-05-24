using System.Buffers;
using History_DataMoex.Clients.Errors;

namespace History_DataMoex.Infrastructure.Buffers
{
    public readonly struct RentedBuffer: IDisposable
    {
        private const int DefaultMaxResponseBytes = 16 * 1024 * 1024;
        readonly byte[] _buffer;
        readonly int _length;
        

        private RentedBuffer(int length, byte[] buffer)
        {
            _length = length;
            _buffer = buffer;
        }
        public ReadOnlySpan<byte> Span => _buffer.AsSpan(0, _length);

        /// <summary>
        /// Read-only memory view над тем же арендованным массивом и длиной, что и Span.
        /// Подходит для API, которым нужен ReadOnlyMemory&lt;byte&gt;, и позволяет передать
        /// содержимое без .ToArray() и без лишнего копирования буфера.
        /// </summary>
        public ReadOnlyMemory<byte> Memory => _buffer.AsMemory(0, _length);
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }

        public static Task<RentedBuffer> RentFromStreamAsync(Stream stream, int lengthArrayFromStreamHttp, CancellationToken cancellationToken)
        {
            return RentFromStreamAsync(stream, lengthArrayFromStreamHttp, DefaultMaxResponseBytes, cancellationToken);
        }

        public static async Task<RentedBuffer> RentFromStreamAsync(Stream stream, int initialBufferSize, long maxResponseBytes, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (maxResponseBytes < 1 || maxResponseBytes > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxResponseBytes), "maxResponseBytes must be between 1 and Int32.MaxValue.");
            }

            int maxBufferSize = (int)maxResponseBytes;
            int normalizedInitialBufferSize = initialBufferSize;
            if (normalizedInitialBufferSize < 1)
            {
                normalizedInitialBufferSize = 1;
            }

            if (normalizedInitialBufferSize > maxBufferSize)
            {
                normalizedInitialBufferSize = maxBufferSize;
            }

            byte[] arr = ArrayPool<byte>.Shared.Rent(normalizedInitialBufferSize);
            int currentPositionInArray = 0;
            try
            {
                while (true)
                {
                    if (currentPositionInArray >= maxBufferSize)
                    {
                        byte[] probe = new byte[1];
                        int overflowBytesRead = await stream.ReadAsync(probe.AsMemory(0, 1), cancellationToken);
                        if (overflowBytesRead != 0)
                        {
                            throw new MoexResponseTooLargeException(maxResponseBytes);
                        }

                        break;
                    }

                    if (currentPositionInArray >= arr.Length)
                    {
                        int nextBufferSize = GetNextBufferSize(arr.Length, maxBufferSize);
                        byte[] nextBuffer = ArrayPool<byte>.Shared.Rent(nextBufferSize);
                        Buffer.BlockCopy(arr, 0, nextBuffer, 0, currentPositionInArray);
                        ArrayPool<byte>.Shared.Return(arr);
                        arr = nextBuffer;
                    }

                    int bytesAvailableInArray = arr.Length - currentPositionInArray;
                    int bytesAllowedByLimit = maxBufferSize - currentPositionInArray;
                    int bytesToRead = bytesAvailableInArray < bytesAllowedByLimit
                        ? bytesAvailableInArray
                        : bytesAllowedByLimit;
                    int bytesRead = await stream.ReadAsync(arr, currentPositionInArray, bytesToRead, cancellationToken);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    currentPositionInArray += bytesRead;
                }

                return new RentedBuffer(currentPositionInArray, arr);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(arr);
                throw;

            }
        }

        private static int GetNextBufferSize(int currentBufferSize, int maxBufferSize)
        {
            if (currentBufferSize >= maxBufferSize)
            {
                return maxBufferSize;
            }

            long doubledBufferSize = (long)currentBufferSize * 2;
            if (doubledBufferSize > maxBufferSize)
            {
                return maxBufferSize;
            }

            return (int)doubledBufferSize;
        }
    }
}