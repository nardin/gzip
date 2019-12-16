using System;
using System.Buffers;
using System.IO;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    class GzipReadFileWorker : BaseWorker<FileChunk>
    {
        private readonly byte[] _defaultHeader = new byte[]{31,139,8,0,0,0,0,0};
        private readonly Stream _stream;

        public GzipReadFileWorker(Stream stream)
        {
            this._stream = stream;
        }

        public override void Run()
        {
            var i = 0;
            var header = new byte[8];
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                count = this._stream.Read(header, 0, 8);
                if (count == 0)
                {
                    break;
                }
                if (header[3] != 1 << 6)
                {
                    throw new InvalidDataException("Incorrect format of block");
                }

                var chunkSize = BitConverter.ToInt32(header, 4);
                var chunk = new FileChunk { Number = i, Data = ArrayPool<byte>.Shared.Rent(chunkSize) };
                _defaultHeader.CopyTo(chunk.Data, 0);
                count = this._stream.Read(chunk.Data, 8, chunkSize-8);
                if (count == 0)
                {
                    break;
                }
                chunk.Length = count;
                cancellationToken.ThrowIfCancellationRequested();
                _outPipe.Add(chunk);
                i++;
            }
        }
    }
}
