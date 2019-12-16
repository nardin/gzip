using Gzip.Lib.Common;
using System;
using System.IO;
using System.Threading;
using Gzip.Lib.Collection;
using Gzip.Lib.Worker;

namespace Gzip.Lib
{
    public class Gzip
    {
        /// <summary>
        /// Compress
        /// </summary>
        /// <param name="inFile">Input file path.</param>
        /// <param name="outFile">Output file path.</param>
        /// <param name="chunkSize">Size of chunk.</param>
        /// <param name="isCompatibility">compatibility rfc1952</param>
        /// <param name="cancellationTokenSource">Cancellation token source.</param>
        public static void Compress(string inFile, string outFile, int chunkSize, bool isCompatibility, CancellationTokenSource cancellationTokenSource = null)
        {
            var size = Environment.ProcessorCount;

            var chunkSizeBytes = chunkSize * 1_048_576; //1MB
            var workFlow = new WorkFlow<FileChunk>(cancellationTokenSource);
            try
            {
                using (var inFileStream = new FileStream(inFile, FileMode.Open))
                {
                    using (var outFileStream = new FileStream(outFile, FileMode.Create))
                    {
                        workFlow
                            .Step(new FileReadWorker(inFileStream, chunkSizeBytes))
                            .Pipe<BlockingPipe<FileChunk>>(size)
                            .Step<CompressWorker>(size)
                            .Pipe<OrderingPipe>(size)
                            .Step(new GzipWriteFileWorker(outFileStream, isCompatibility))
                            .Run();
                    }
                }
            }
            catch (AggregateException ex)
            {
                workFlow.Cancel();
                throw ex.Flatten();
            }
        }

        /// <summary>
        /// Decompress.
        /// </summary>
        /// <param name="inFile">Input file path.</param>
        /// <param name="outFile">Output file path.</param>
        /// <param name="cancellationTokenSource">Cancellation token source.</param>
        public static void Decompress(string inFile, string outFile, CancellationTokenSource cancellationTokenSource = null)
        {
            var workFlow = new WorkFlow<FileChunk>(cancellationTokenSource);
            var readHeader = new byte[4];
            var size = Environment.ProcessorCount;

            try
            {
                using (var inFileStream = new FileStream(inFile, FileMode.Open))
                {
                    
                    using (var outFileStream = new FileStream(outFile, FileMode.Create))
                    {

                        inFileStream.Read(readHeader, 0, readHeader.Length);
                        inFileStream.Position = 0;
                        if (readHeader[3] == 1 << 6)
                        {
                            workFlow
                                .Step(new GzipReadFileWorker(inFileStream))
                                .Pipe<BlockingPipe<FileChunk>>(size)
                                .Step<DecompressWorker>(size)
                                .Pipe<OrderingPipe>(size)
                                .Step(new FileWriteWorker(outFileStream))
                                .Run();
                        }
                        else
                        {
                            workFlow
                                .Step(new DecompressSingleThreadWorker(inFileStream, outFileStream))
                                .Run();
                        }
                    }
                }
            }
            catch (AggregateException ex)
            {
                workFlow.Cancel();
                throw ex.Flatten();
            }
        }
    }
}
