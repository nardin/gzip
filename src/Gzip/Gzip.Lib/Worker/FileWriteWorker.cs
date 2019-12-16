using System.Buffers;
using System.IO;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    class FileWriteWorker : BaseWorker<FileChunk>
    {
        private readonly Stream _outStream;
        public FileWriteWorker(Stream outStream)
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
            }
        }
    }
}
