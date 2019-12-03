using System;
using System.Buffers;
using System.IO;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    internal class FileReadWorker : BaseWorker<FileChunk>
    {
        private readonly Stream _stream;
        private readonly int _chunkSize;

        public FileReadWorker(Stream stream, int chunkSize)
        {
            this._stream = stream;
            this._chunkSize = chunkSize;
        }

        public override void Run()
        {
            var i = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chunk = new FileChunk { Number = i, Data = ArrayPool<byte>.Shared.Rent(this._chunkSize) };
                var count = this._stream.Read(chunk.Data, 0, chunk.Data.Length);
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
