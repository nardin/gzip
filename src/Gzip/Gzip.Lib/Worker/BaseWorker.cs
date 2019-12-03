using System;
using System.Threading;
using Gzip.Lib.Collection;

namespace Gzip.Lib.Worker
{
    internal abstract class BaseWorker<T> : IWorker<T>
    {
        protected IPipe<T> _inPipe;
        protected IPipe<T> _outPipe;
        protected CancellationToken cancellationToken;

        public void SetInPipe(IPipe<T> pipe)
        {
            this._inPipe = pipe;
        }

        public void SetOutPipe(IPipe<T> pipe)
        {
            this._outPipe = pipe;
            this._outPipe.Attach();
        }

        public void SetCancellationToken(CancellationToken token)
        {
            this.cancellationToken = token;
        }

        public abstract void Run();
        public void Dispose()
        {
            _outPipe?.Deattach();
        }
    }
}
