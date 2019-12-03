using System;
using System.Threading;
using Gzip.Lib.Collection;

namespace Gzip.Lib.Worker
{
    interface IWorker<T> : IDisposable
    {
        void SetInPipe(IPipe<T> pipe);
        void SetOutPipe(IPipe<T> pipe);

        void SetCancellationToken(CancellationToken token);

        void Run();
    }
}
