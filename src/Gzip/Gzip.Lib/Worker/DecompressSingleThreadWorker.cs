using System.IO;
using System.IO.Compression;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    internal class DecompressSingleThreadWorker : BaseWorker<FileChunk>
    {
        private readonly Stream _inStream;
        private readonly Stream _outStream;

        public DecompressSingleThreadWorker(Stream inStream, Stream outStream)
        {
            this._inStream = inStream;
            this._outStream = outStream;
        }

        public override void Run()
        {
            using (var decompressionStream = new GZipStream(this._inStream, CompressionMode.Decompress))
            {
                cancellationToken.ThrowIfCancellationRequested();
                decompressionStream.CopyTo(_outStream);
            }
        }
    }
}
