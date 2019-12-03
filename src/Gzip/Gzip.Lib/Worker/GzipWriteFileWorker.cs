using System;
using System.Buffers;
using System.IO;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    internal class GzipWriteFileWorker: BaseWorker<FileChunk>
    {
        private readonly Stream _outStream;
        public GzipWriteFileWorker(Stream outStream)
        {
            this._outStream = outStream;
        }

        public override void Run()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chunk = _inPipe.Take();
                if (chunk == default(FileChunk))
                {
                    this._outStream.Close();
                    break;
                }
                this._outStream.Write(chunk.Data, 0, chunk.Length);
                ArrayPool<byte>.Shared.Return(chunk.Data);
            }
        }
    }
}
