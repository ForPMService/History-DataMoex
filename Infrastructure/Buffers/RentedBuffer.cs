using System.Buffers;

namespace History_DataMoex.Infrastructure.Buffers
{
    public readonly struct RentedBuffer: IDisposable
    {
        readonly byte[] _buffer;
        readonly int _length;
        

        private RentedBuffer(int length, byte[] buffer)
        {
            _length = length;
            _buffer = buffer;
        }
        public ReadOnlySpan<byte> Span => _buffer.AsSpan(0, _length);
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }

        public static async Task<RentedBuffer> RentFromStreamAsync(Stream stream, int lengthArrayFromStreamHttp, CancellationToken cancellationToken)
        {
            var arr = ArrayPool<byte>.Shared.Rent(lengthArrayFromStreamHttp);
            int sizeLatsPortionByteInArray = 0;
            int currentPositionInArray = 0;
            try
            {

                while (currentPositionInArray < lengthArrayFromStreamHttp)
                {
                    sizeLatsPortionByteInArray = await stream.ReadAsync(arr, currentPositionInArray, lengthArrayFromStreamHttp - currentPositionInArray, cancellationToken);
                    currentPositionInArray += sizeLatsPortionByteInArray;
                    if (sizeLatsPortionByteInArray == 0)
                    {
                        break;
                    }

                }

                return new RentedBuffer(currentPositionInArray, arr);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(arr);
                throw;

            }
        }
    }
}