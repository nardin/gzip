﻿using System;
using System.Buffers;
using System.IO;
using Gzip.Lib.Common;

namespace Gzip.Lib.Worker
{
    internal class GzipWriteFileWorker: BaseWorker<FileChunk>
    {
        private readonly Stream _outStream;
        private readonly bool _isCompatibility;
        public GzipWriteFileWorker(Stream outStream, bool isCompatibility)
        {
            this._outStream = outStream;
            this._isCompatibility = isCompatibility;
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

                if (!_isCompatibility)
                {
                    chunk.Data[3] = 1 << 6;
                    var length = BitConverter.GetBytes(chunk.Length);
                    length.CopyTo(chunk.Data, 4);
                }

                this._outStream.Write(chunk.Data, 0, chunk.Length);
                ArrayPool<byte>.Shared.Return(chunk.Data);
            }
        }
    }
}
