using System.Buffers;
using System.IO;
using System.IO.Compression;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    class DecompressWorker : BaseWorker<FileChunk>
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

                using (var baseStream = new MemoryStream(chunk.Data, 0, chunk.Length))
                {
                    using (var gzipStream = new GZipStream(baseStream, CompressionMode.Decompress))
                    {
                        

                        using (var resultStream = new MemoryStream())
                        {
                            gzipStream.CopyTo(resultStream);
                            var data = resultStream.ToArray();
                            var outChunk = new FileChunk()
                            {
                                Number = chunk.Number,
                                Data = data,
                                Length = data.Length

                            };
                            ArrayPool<byte>.Shared.Return(chunk.Data);
                            _outPipe.Add(outChunk);
                        }
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
