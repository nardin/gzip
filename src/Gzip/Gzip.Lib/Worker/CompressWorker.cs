using Gzip.Lib.Common;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace Gzip.Lib.Worker
{
    internal class CompressWorker: BaseWorker<FileChunk>
    {
        public override void Run()
        {
            while (true)
            {
                var chunk = _inPipe.Take();
                if (chunk == default(FileChunk))
                {
                    break;
                }

                var streamLength = (int) (chunk.Length * 1.5);
                var data = ArrayPool<byte>.Shared.Rent(streamLength);
                using (var baseStream = new MemoryStream(data, 0, data.Length, true, true))
                {
                    using (var gzipStream = new GZipStream(baseStream, CompressionMode.Compress, true))
                    {
                        gzipStream.Write(chunk.Data, 0, chunk.Length);
                        gzipStream.Flush();
                        gzipStream.Close(); // stream close for write CRC to base stream.

                        ArrayPool<byte>.Shared.Return(chunk.Data);

                        var outChunk = new FileChunk()
                        {
                            Number = chunk.Number,
                            Data = baseStream.GetBuffer(),
                            Length = (int) baseStream.Position

                        };

                        _outPipe.Add(outChunk);
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
