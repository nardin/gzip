using System;
using System.Threading;
using Gzip.Lib.Collection;
using Gzip.Lib.Common;
using Gzip.Lib.Worker;

namespace Gzip.Lib.Tests.Common
{
    internal class EndlessWorker : BaseWorker<FileChunk>
    {
        public override void Run()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    internal class WaitWorker : BaseWorker<FileChunk>
    {
        private object _lockonject = new object();
        public override void Run()
        {
            lock (_lockonject)
            {
                Monitor.Wait(_lockonject);
            }
        }
    }

    internal class ExeptionWorker : BaseWorker<FileChunk>
    {
        private object _lockonject = new object();
        public override void Run()
        {
            Thread.Sleep(100);
            throw new NullReferenceException();
        }
    }


    internal class FakePipe : IPipe<FileChunk>
    {
        public FakePipe(int boundedCapacity) : base()
        {

        }

        public void Attach()
        {
            
        }

        public void Deattach()
        {
            
        }

        public bool IsCompleted { get; }
        public void Add(FileChunk t)
        {
            
        }

        public FileChunk Take()
        {
            return new FileChunk();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }
    }
}
